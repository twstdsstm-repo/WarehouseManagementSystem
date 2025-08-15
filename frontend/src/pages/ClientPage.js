import React, { useState, useEffect } from 'react';
import { fetchClients, addClient, archiveClient, unarchiveClient, deleteClient, updateClient } from '../services/api'; 

const ClientPage = () => {
  const [clients, setClients] = useState([]); 
  const [newClient, setNewClient] = useState({ name: '', address: '' }); 
  const [editClient, setEditClient] = useState(null); 
  const [sortOrder, setSortOrder] = useState('asc'); 

  
  const loadClients = async () => {
    try {
      const response = await fetchClients(); 
      const sortedClients = sortClients(response.data, sortOrder); 
      setClients(sortedClients); 
    } catch (error) {
      console.error('Error fetching clients:', error);
    }
  };

  
  const sortClients = (clients, order) => {
    return clients.sort((a, b) => {
      if (order === 'asc') {
        return a.id - b.id; 
      } else {
        return b.id - a.id; 
      }
    });
  };

  useEffect(() => {
    loadClients(); 
  }, [sortOrder]); 

  
  const handleNewClientChange = (e) => {
    const { name, value } = e.target;
    setNewClient((prevClient) => ({
      ...prevClient,
      [name]: value,
    }));
  };

  
  const handleEditClientChange = (e) => {
    const { name, value } = e.target;
    setEditClient((prevClient) => ({
      ...prevClient,
      [name]: value,
    }));
  };

  
  const handleAddClient = async (e) => {
    e.preventDefault();

    try {
      await addClient(newClient); 

      
      window.location.reload(); 

      
      setNewClient({ name: '', address: '' });
    } catch (error) {
      console.error('Error adding client:', error);
    }
  };

  
  const handleUpdateClient = async (e) => {
    e.preventDefault();

    try {
      await updateClient(editClient.id, editClient); 

      
      loadClients(); 

      
      setEditClient(null);
    } catch (error) {
      console.error('Error updating client:', error);
    }
  };

  
  const handleArchiveClient = async (id) => {
    try {
      await archiveClient(id); 

      
      loadClients(); 
    } catch (error) {
      console.error('Error archiving client:', error);
    }
  };

  
  const handleUnarchiveClient = async (id) => {
    try {
      await unarchiveClient(id); 

      
      loadClients(); 
    } catch (error) {
      console.error('Error unarchiving client:', error);
    }
  };

  
  const handleDeleteClient = async (id) => {
    try {
      await deleteClient(id); 

      
      loadClients(); 
    } catch (error) {
      console.error('Error deleting client:', error);
    }
  };

  
  const handleSortChange = (e) => {
    setSortOrder(e.target.value); 
  };

  return (
    <div className="client-detail-container">
      <h1>Клиенты</h1>

      <form onSubmit={handleAddClient} className="add-client-form">
        <input
          type="text"
          name="name"
          value={newClient.name}
          onChange={handleNewClientChange}
          placeholder="Наименование клиента"
          required
        />
        <input
          type="text"
          name="address"
          value={newClient.address}
          onChange={handleNewClientChange}
          placeholder="Адрес клиента (необязательно)"
        />
        <button type="submit">Добавить клиента</button>
      </form>

      <div>
        <label>Сортировать по:</label>
        <select onChange={handleSortChange} value={sortOrder}>
          <option value="asc">По возрастанию ID</option>
          <option value="desc">По убыванию ID</option>
        </select>
      </div>

      {editClient && (
        <form onSubmit={handleUpdateClient} className="edit-client-form">
          <h3>Редактировать клиента</h3>
          <input
            type="text"
            name="name"
            value={editClient.name}
            onChange={handleEditClientChange}
            placeholder="Наименование клиента"
            required
          />
          <input
            type="text"
            name="address"
            value={editClient.address}
            onChange={handleEditClientChange}
            placeholder="Адрес клиента"
          />
          <button type="submit">Обновить клиента</button>
          <button type="button" onClick={() => setEditClient(null)}>Отменить</button>
        </form>
      )}

      <h3>Список клиентов</h3>
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Наименование</th>
            <th>Адрес</th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {clients.map((client) => (
            <tr key={client.id}>
              <td>{client.id}</td>
              <td>{client.name}</td>
              <td>{client.address}</td>
              <td>{client.isArchived ? 'Архивирован' : 'Активен'}</td>
              <td>
                <button onClick={() => setEditClient(client)}>Изменить</button>

                {!client.isArchived && (
                  <button onClick={() => handleArchiveClient(client.id)}>
                    Архивировать
                  </button>
                )}

                {client.isArchived && (
                  <button onClick={() => handleUnarchiveClient(client.id)}>
                    Разархивировать
                  </button>
                )}

                <button onClick={() => handleDeleteClient(client.id)}>
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

export default ClientPage;
