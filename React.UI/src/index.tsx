import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import UploadPage from './pages/UploadPage';
import PortfolioPage from './pages/PortfolioPage';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/"            element={<UploadPage />} />
        <Route path="/portfolio"   element={<PortfolioPage />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);