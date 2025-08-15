import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import ReceiptPage from './pages/ReceiptPage';
import BalancePage from './pages/BalancePage'; 
import ClientPage from './pages/ClientPage';    
import ResourcePage from './pages/ResourcePage';
import ShipmentPage from './pages/ShipmentPage';
import UnitMeasurementPage from './pages/UnitPage';

const App = () => {
  return (
    <Router>
      <div>
        <h1>Управление складом</h1>
        <nav>
          <ul>
            <li><a href="/receipt">Документ поступления</a></li>
            <li><a href="/balance">Баланс</a></li>
            <li><a href="/client">Клиенты</a></li>
            <li><a href="/resource">Ресурсы</a></li>
            <li><a href="/shipment">Отгрузка</a></li>
            <li><a href="/unit-measurement">Единицы измерения</a></li>
          </ul>
        </nav>

        <Routes>
          <Route path="/receipt" element={<ReceiptPage />} />
          <Route path="/balance" element={<BalancePage />} />
          <Route path="/client" element={<ClientPage />} />
          <Route path="/resource" element={<ResourcePage />} />
          <Route path="/shipment" element={<ShipmentPage />} />
          <Route path="/unit-measurement" element={<UnitMeasurementPage />} />
        </Routes>
      </div>
    </Router>
  );
};

export default App;
