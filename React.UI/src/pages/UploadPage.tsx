import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Config } from '../configHelper';
import './UploadPage.css';

export default function UploadPage() {
  const [file, setFile] = useState<File>();
  const [busy, setBusy] = useState(false);
  const nav = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!file) return;

    const fd = new FormData();
    fd.append('file', file);
    setBusy(true);

    const res = await fetch(Config.api.upload, { method: 'PUT', body: fd });
    setBusy(false);

    if (!res.ok) {
      alert(await res.text());
      return;
    }

    const payload = await res.json();   
    nav('/portfolio', { state: payload });
  }

  return (
    <main className="page">
      <h1>Upload your portfolio file</h1>

      <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-4">
        <input
          type="file"
          accept=".csv,.json"      // adjust to your spec
          onChange={e => setFile(e.target.files?.[0])}
        />

        <button type="submit" disabled={!file || busy}>
          {busy ? 'Uploading…' : 'Upload & View'}
        </button>
      </form>
    </main>
  );
}