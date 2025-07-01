import { useEffect, useState, useRef } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Config } from "../helpers/ConfigHelper";
import { CoinRow } from "../types/CoinRowType";
import { updateAiInfo, refreshInformation } from "../services/PortfolioService";
import "./PortfolioPage.css";

export default function PortfolioPage() {
  const navigate = useNavigate();
  const { state } = useLocation();
  const initialData = state as CoinRow[] | undefined;
  const [rows, setRows] = useState<CoinRow[] | undefined>();
  const [error, setErr] = useState<string>();

  const firstRun = useRef(true);
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!initialData) {
      navigate("/");
    }
  }, [initialData, navigate]);

  useEffect(() => {
    if (firstRun.current === true) {
      updateAiInfo(initialData)
        .then((updated) => {
          if (updated) {
            setRows(updated);
            setErr(undefined);
          }
        })
        .catch((e) => {
          setErr((e as Error).message);
        });
    }

    firstRun.current = false;
  }, [initialData]);

  useEffect(() => {
    async function timerUpdate() {
      try {
        var updatedCoins = await refreshInformation();

        setRows((prevRows) => {
          const base = prevRows ?? [];
          return updatedCoins.map((row, i) => ({
            ...row,
            coinInformation: base[i]?.coinInformation ?? "",
          }));
        });
        setErr(undefined);
      } catch (err) {
        console.log(err);
        setErr((err as Error).message);
      } finally {
        if (timer.current) clearTimeout(timer.current);
        timer.current = setTimeout(timerUpdate, Config.pollIntervalMs);
      }
    }

    timer.current = setTimeout(timerUpdate, Config.pollIntervalMs);
    return () => {
      if (timer.current) clearTimeout(timer.current);
    };
  }, []);

  return (
    <main className="page">
      <button onClick={() => navigate("/")} className="mb-4">
        ↩ Upload another file
      </button>

      <h1>Your Crypto Portfolio</h1>

      {error && <p style={{ color: "red" }}>Failed to refresh: {error}</p>}

      {rows ? (
        <table>
          <thead>
            <tr>
              <th>Coin</th>
              <th>CoinCount</th>
              <th>Bought Price&nbsp;(USD)</th>
              <th>Current Coin Price&nbsp;(USD)</th>
              <th>Total Holdings Value&nbsp;(USD)</th>
              <th>Profit&nbsp;%</th>
              <th>Tips and information</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.coinCode}>
                <td>{r.coinCode}</td>
                <td>{r.coinCount}</td>
                <td>{r.boughtValue}</td>
                <td>{r.currentCoinValue}</td>
                <td>{r.currentValue.toLocaleString()}</td>
                <td
                  className={r.percentageChange < 0 ? "negative" : "positive"}
                >
                  {r.percentageChange.toFixed(5)}
                </td>
                <td>{r.coinInformation}</td>
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <p>Loading …</p>
      )}
    </main>
  );
}
