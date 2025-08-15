import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import { fetchResources, fetchUnits, fetchFilteredBalances } from '../services/api';

const BalancePage = () => {
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [balances, setBalances] = useState([]);

  const [filters, setFilters] = useState({
    resourceIds: [],
    unitOfMeasurementIds: [],
  });

  
  const loadData = async () => {
    try {
      const [resourcesResponse, unitsResponse] = await Promise.all([
        fetchResources(),
        fetchUnits(),
      ]);
      setResources(resourcesResponse.data || []);
      setUnits(unitsResponse.data || []);
    } catch (error) {
      console.error('Ошибка при загрузке данных:', error);
    }
  };

  
  const loadBalances = async () => {
    try {
      const response = await fetchFilteredBalances(filters);
      setBalances(response || []);
    } catch (error) {
      console.error('Ошибка при загрузке баланса:', error);
    }
  };

  useEffect(() => {
    loadData();
    loadBalances();
  }, []);

  
  const handleFilterChange = (selectedOptions, name) => {
    const selectedValues = selectedOptions ? selectedOptions.map(opt => opt.value) : [];
    setFilters(prev => ({
      ...prev,
      [name]: selectedValues,
    }));
  };

  const resetFilters = () => {
    setFilters({ resourceIds: [], unitOfMeasurementIds: [] });
  };

  
  const applyFilters = () => {
    loadBalances();
  };

  
  const resourceOptions = resources.map(r => ({ value: r.id, label: r.name }));
  const unitOptions = units.map(u => ({ value: u.id, label: u.name }));

  const customStyles = {
    control: base => ({ ...base, width: '250px', minWidth: '200px' }),
    menu: base => ({ ...base, maxHeight: '200px', overflowY: 'auto' }),
  };

  return (
    <div className="balance-page-container">
      <h1>Баланс</h1>

      <h3>Фильтрация</h3>
      <div style={{ marginBottom: '10px' }}>
        <label>Ресурсы:</label>
        <Select
          isMulti
          value={resourceOptions.filter(opt => filters.resourceIds.includes(opt.value))}
          onChange={selected => handleFilterChange(selected, 'resourceIds')}
          options={resourceOptions}
          placeholder="Выберите ресурсы"
          styles={customStyles}
        />
      </div>

      <div style={{ marginBottom: '10px' }}>
        <label>Единицы измерения:</label>
        <Select
          isMulti
          value={unitOptions.filter(opt => filters.unitOfMeasurementIds.includes(opt.value))}
          onChange={selected => handleFilterChange(selected, 'unitOfMeasurementIds')}
          options={unitOptions}
          placeholder="Выберите единицы измерения"
          styles={customStyles}
        />
      </div>

      <div style={{ marginBottom: '20px' }}>
        <button onClick={applyFilters} style={{ marginRight: '10px' }}>Применить фильтрацию</button>
        <button onClick={resetFilters}>Сбросить фильтрацию</button>
      </div>

      <h3>Список баланса</h3>
      <table>
        <thead>
          <tr>
            <th>Ресурс</th>
            <th>Единица измерения</th>
            <th>Количество</th>
          </tr>
        </thead>
        <tbody>
          {balances.length > 0 ? (
            balances.map(balance => (
              <tr key={balance.id}>
                <td>{resources.find(r => r.id === balance.resourceId)?.name}</td>
                <td>{units.find(u => u.id === balance.unitOfMeasurementId)?.name}</td>
                <td>{balance.quantity}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="3">Нет данных для отображения</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
};

export default BalancePage;
