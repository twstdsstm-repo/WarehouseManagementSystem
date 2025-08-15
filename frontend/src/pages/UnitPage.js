import React, { useState, useEffect } from 'react';
import { fetchUnits, addUnit, archiveUnit, unarchiveUnit, deleteUnit, updateUnit } from '../services/api'; 

const UnitMeasurementPage = () => {
  const [units, setUnits] = useState([]); 
  const [newUnit, setNewUnit] = useState({ name: '' }); 
  const [editUnit, setEditUnit] = useState(null); 
  const [sortOrder, setSortOrder] = useState('asc'); 

  
  const loadUnits = async () => {
    try {
      const response = await fetchUnits(); 
      const sortedUnits = sortUnits(response.data, sortOrder); 
      setUnits(sortedUnits); 
    } catch (error) {
      console.error('Error fetching units:', error);
    }
  };

  
  const sortUnits = (units, order) => {
    return units.sort((a, b) => {
      if (order === 'asc') {
        return a.id - b.id; 
      } else {
        return b.id - a.id; 
      }
    });
  };

  useEffect(() => {
    loadUnits(); 
  }, [sortOrder]); 

  
  const handleNewUnitChange = (e) => {
    const { name, value } = e.target;
    setNewUnit((prevUnit) => ({
      ...prevUnit,
      [name]: value,
    }));
  };

  
  const handleEditUnitChange = (e) => {
    const { name, value } = e.target;
    setEditUnit((prevUnit) => ({
      ...prevUnit,
      [name]: value,
    }));
  };

  
  const handleAddUnit = async (e) => {
    e.preventDefault();

    try {
      await addUnit(newUnit); 

      
      window.location.reload(); 

      
      setNewUnit({ name: '' });
    } catch (error) {
      console.error('Error adding unit:', error);
    }
  };

  
  const handleUpdateUnit = async (e) => {
    e.preventDefault();

    try {
      await updateUnit(editUnit.id, editUnit); 

      
      loadUnits(); 

      
      setEditUnit(null);
    } catch (error) {
      console.error('Error updating unit:', error);
    }
  };

  
  const handleArchiveUnit = async (id) => {
    try {
      await archiveUnit(id); 

      
      loadUnits(); 
    } catch (error) {
      console.error('Error archiving unit:', error);
    }
  };

  
  const handleUnarchiveUnit = async (id) => {
    try {
      await unarchiveUnit(id); 

      
      loadUnits(); 
    } catch (error) {
      console.error('Error unarchiving unit:', error);
    }
  };

  
  const handleDeleteUnit = async (id) => {
    try {
      await deleteUnit(id); 

      
      loadUnits(); 
    } catch (error) {
      console.error('Error deleting unit:', error);
    }
  };

  
  const handleSortChange = (e) => {
    setSortOrder(e.target.value); 
  };

  return (
    <div className="unit-detail-container">
      <h1>Единицы измерения</h1>
      <form onSubmit={handleAddUnit} className="add-unit-form">
        <input
          type="text"
          name="name"
          value={newUnit.name}
          onChange={handleNewUnitChange}
          placeholder="Название единицы измерения"
          required
        />
        <button type="submit">Добавить единицу измерения</button>
      </form>

      <div>
        <label>Сортировать по:</label>
        <select onChange={handleSortChange} value={sortOrder}>
          <option value="asc">По возрастанию ID</option>
          <option value="desc">По убыванию ID</option>
        </select>
      </div>

      {editUnit && (
        <form onSubmit={handleUpdateUnit} className="edit-unit-form">
          <h3>Редактировать единицу измерения</h3>
          <input
            type="text"
            name="name"
            value={editUnit.name}
            onChange={handleEditUnitChange}
            placeholder="Название единицы измерения"
            required
          />
          <button type="submit">Обновить единицу измерения</button>
          <button type="button" onClick={() => setEditUnit(null)}>Отменить</button>
        </form>
      )}

      <h3>Список единиц измерения</h3>
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Наименование</th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {units.map((unit) => (
            <tr key={unit.id}>
              <td>{unit.id}</td>
              <td>{unit.name}</td>
              <td>{unit.isArchived ? 'Архивирован' : 'Активен'}</td>
              <td>
                <button onClick={() => setEditUnit(unit)}>Изменить</button>

                {!unit.isArchived && (
                  <button onClick={() => handleArchiveUnit(unit.id)}>
                    Архивировать
                  </button>
                )}

                {unit.isArchived && (
                  <button onClick={() => handleUnarchiveUnit(unit.id)}>
                    Разархивировать
                  </button>
                )}

                <button onClick={() => handleDeleteUnit(unit.id)}>
                  Удалить
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UnitMeasurementPage;
