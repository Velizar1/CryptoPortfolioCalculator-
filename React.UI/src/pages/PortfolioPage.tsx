import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Config } from '../configHelper';
import './PortfolioPage.css';

interface CoinRow {
   coinCode : string;
   coinCount : number;
   boughtValue : number;
   currentValue : number;
   percentageChange  : number;   
}

export default function PortfolioPage() {
  const nav = useNavigate();
  const { state } = useLocation();  
  const data = state as CoinRow[] | undefined;

  const [rows, setRows] = useState<CoinRow[] | undefined>(data);
  const [error, setErr] = useState<string>();

  

 useEffect(() => {
    if (!rows && !error) {
        nav('/');
        return;
    }
    let timerId: ReturnType<typeof setTimeout>;
   
    async function fetchOnce() {
      try {
        const res = await fetch(Config.api.refresh, { method: 'GET'});   
        if (!res.ok) throw new Error(await res.text());
        const json: CoinRow[] = await res.json();
        setRows(json);                                 
        setErr(undefined);                              
      } catch (err) {
        setErr((err as Error).message);
      } 
    }   

    timerId = setTimeout(fetchOnce, Config.pollIntervalMs);
    return () => clearTimeout(timerId);
  }, [rows, error]);

   return (
    <main className="page">
      <button onClick={() => nav('/')} className="mb-4">
        ↩ Upload another file
      </button>

      <h1>Your Crypto Portfolio</h1>

      {error && (
        <p style={{ color: 'red' }}>
          Failed to refresh: {error}
        </p>
      )}

      {rows ? (
        <table>
          <thead>
            <tr>
              <th>Coin</th>
              <th>CoinCount</th>
              <th>Bought Price&nbsp;(USD)</th>
              <th>Current Price&nbsp;(USD)</th>
              <th>Profit&nbsp;%</th>
            </tr>
          </thead>
          <tbody>
            {rows.map(r => (
              <tr key={r.coinCode}>
                <td>{r.coinCode}</td>
                <td>{r.coinCount}</td>
                <td>{r.boughtValue}</td>
                <td>{r.currentValue.toLocaleString()}</td>
                <td className={ r.percentageChange < 0 ? 'negative' : 'positive'}>
                    {r.percentageChange.toFixed(5)}
                </td>
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