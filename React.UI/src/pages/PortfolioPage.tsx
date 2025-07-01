import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { useRef } from "react";
import { Config } from "../helpers/configHelper";
import OpenAI from "openai";
import "./PortfolioPage.css";
import type {
  ResponseInput,
  ResponseInputItem,
} from "openai/resources/responses/responses";

const inputMessage: string =
  "Write a one-sentence information about the stabiliti of the coin and is it a good time to buy it. Return only a single new line at the end of every line and no other in the response content.";

interface CoinRow {
  coinCode: string;
  coinCount: number;
  boughtValue: number;
  currentValue: number;
  currentCoinValue: number;
  percentageChange: number;
  coinInformation: string;
}

const client = new OpenAI({
  apiKey: Config.openAiKey,
  dangerouslyAllowBrowser: true,
});

const buildMessage = (content: string): ResponseInputItem => ({
  role: "user",
  content,
});

export default function PortfolioPage() {
  const nav = useNavigate();
  const { state } = useLocation();
  const data = state as CoinRow[] | undefined;
  const [rows, setRows] = useState<CoinRow[] | undefined>();
  const [error, setErr] = useState<string>();

  const firstRun = useRef(true);
  const timer = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (firstRun.current === true) {
      updateAiInfo(data);
    }
    firstRun.current = false;

    if (!data && !error) {
      nav("/");
      return;
    }

    async function updateAiInfo(json: CoinRow[] | undefined) {
      if (!json) return;

      let rowsWithInfo: CoinRow[]
      const aiInput: ResponseInput = [
        {
          role: "developer",
          content: `${inputMessage}`,
        },
        ...json.map((row) => buildMessage(`Coin: ${row.coinCode}.`)),
      ];

      try {
        if (!client.apiKey || client.apiKey === "") {
          rowsWithInfo = json.map((row, i) => ({
            ...row,
            coinInformation: "Not available",
          }));

        } else {
          const aiRes = await client.responses.create({
            model: "gpt-4.1",
            input: aiInput,
            stream: false,
          });

          const summaries = aiRes.output_text
            .trim()
            .replace(/(?:\r?\n){2,}/g, "\n")
            .split("\n");

          rowsWithInfo = json.map((row, i) => ({
            ...row,
            coinInformation: summaries[i] ?? "",
          }));
        }

        setRows(rowsWithInfo);
        setErr(undefined);
      } catch (err) {
        setErr((err as Error).message);
      }
    }

    async function refreshInformation() {
      try {
        const res = await fetch(Config.api.refresh, { method: "GET", credentials: "include" });
        if (!res.ok) throw new Error(await res.text());
        const json: CoinRow[] = await res.json();

        setRows((prevRows) => {
          const base = prevRows ?? [];
          return json.map((row, i) => ({
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
        timer.current = setTimeout(refreshInformation, Config.pollIntervalMs);
      }
    }

    timer.current = setTimeout(refreshInformation, Config.pollIntervalMs);
    return () => {
      if (timer.current) clearTimeout(timer.current);
    };
  }, []);

  return (
    <main className="page">
      <button onClick={() => nav("/")} className="mb-4">
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
