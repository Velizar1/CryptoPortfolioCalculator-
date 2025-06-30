const apiBase = process.env.REACT_APP_API_BASE ?? '';
const pollIntervalMs = Number(process.env.REACT_APP_POLL_INTERVAL ?? '300000');
const uriUpload = process.env.REACT_APP_API_UPLOAD ?? '';
const uriRefresh = process.env.REACT_APP_API_REFRESH ?? '';
const openAiApiKey = process.env.REACT_APP_OPENAI_API_KEY ?? '';

if (!apiBase)
  throw new Error('REACT_APP_API_BASE is missing in .env');

export const Config = {
  api: {
    upload : `${apiBase}${uriUpload}`,
    refresh : `${apiBase}${uriRefresh}`
  },
  pollIntervalMs,
  openAiKey : openAiApiKey
};