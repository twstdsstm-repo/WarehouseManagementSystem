import React, { useState, useEffect } from 'react';
import { fetchResources, addResource, archiveResource, unarchiveResource, deleteResource, updateResource } from '../services/api'; 

const ResourcePage = () => {
  const [resources, setResources] = useState([]); 
  const [newResource, setNewResource] = useState({ name: '' }); 
  const [editResource, setEditResource] = useState(null); 
  const [sortOrder, setSortOrder] = useState('asc'); 

  
  const loadResources = async () => {
    try {
      const response = await fetchResources(); 
      const sortedResources = sortResources(response.data, sortOrder); 
      setResources(sortedResources); 
    } catch (error) {
      console.error('Error fetching resources:', error);
    }
  };

  
  const sortResources = (resources, order) => {
    return resources.sort((a, b) => {
      if (order === 'asc') {
        return a.id - b.id; 
      } else {
        return b.id - a.id; 
      }
    });
  };

  useEffect(() => {
    loadResources(); 
  }, [sortOrder]); 

  
  const handleNewResourceChange = (e) => {
    const { name, value } = e.target;
    setNewResource((prevResource) => ({
      ...prevResource,
      [name]: value,
    }));
  };

  
  const handleEditResourceChange = (e) => {
    const { name, value } = e.target;
    setEditResource((prevResource) => ({
      ...prevResource,
      [name]: value,
    }));
  };

  
  const handleAddResource = async (e) => {
    e.preventDefault();

    try {
      await addResource(newResource); 

      
      window.location.reload(); 

      
      setNewResource({ name: '' });
    } catch (error) {
      console.error('Error adding resource:', error);
    }
  };

  
  const handleUpdateResource = async (e) => {
    e.preventDefault();

    try {
      await updateResource(editResource.id, editResource); 

      
      loadResources(); 

      
      setEditResource(null);
    } catch (error) {
      console.error('Error updating resource:', error);
    }
  };

  
  const handleArchiveResource = async (id) => {
    try {
      await archiveResource(id); 

      
      loadResources(); 
    } catch (error) {
      console.error('Error archiving resource:', error);
    }
  };

  
  const handleUnarchiveResource = async (id) => {
    try {
      await unarchiveResource(id); 

      
      loadResources(); 
    } catch (error) {
      console.error('Error unarchiving resource:', error);
    }
  };

  
  const handleDeleteResource = async (id) => {
    try {
      await deleteResource(id); 

      
      loadResources(); 
    } catch (error) {
      console.error('Error deleting resource:', error);
    }
  };

  
  const handleSortChange = (e) => {
    setSortOrder(e.target.value); 
  };

  return (
    <div className="resource-detail-container">
      <h1>Ресурсы</h1>

      <form onSubmit={handleAddResource} className="add-resource-form">
        <input
          type="text"
          name="name"
          value={newResource.name}
          onChange={handleNewResourceChange}
          placeholder="Название ресурса"
          required
        />
        <button type="submit">Добавить ресурс</button>
      </form>

      <div>
        <label>Сортировать по:</label>
        <select onChange={handleSortChange} value={sortOrder}>
          <option value="asc">По возрастанию ID</option>
          <option value="desc">По убыванию ID</option>
        </select>
      </div>

      {editResource && (
        <form onSubmit={handleUpdateResource} className="edit-resource-form">
          <h3>Редактировать ресурс</h3>
          <input
            type="text"
            name="name"
            value={editResource.name}
            onChange={handleEditResourceChange}
            placeholder="Название ресурса"
            required
          />
          <button type="submit">Обновить ресурс</button>
          <button type="button" onClick={() => setEditResource(null)}>Отменить</button>
        </form>
      )}

      <h3>Список ресурсов</h3>
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
          {resources.map((resource) => (
            <tr key={resource.id}>
              <td>{resource.id}</td>
              <td>{resource.name}</td>
              <td>{resource.isArchived ? 'Архивирован' : 'Активен'}</td>
              <td>
                <button onClick={() => setEditResource(resource)}>Изменить</button>

                {!resource.isArchived && (
                  <button onClick={() => handleArchiveResource(resource.id)}>
                    Архивировать
                  </button>
                )}

                {resource.isArchived && (
                  <button onClick={() => handleUnarchiveResource(resource.id)}>
                    Разархивировать
                  </button>
                )}

                <button onClick={() => handleDeleteResource(resource.id)}>
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

export default ResourcePage;
