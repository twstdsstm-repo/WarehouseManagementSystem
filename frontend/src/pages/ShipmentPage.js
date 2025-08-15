import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import {
  fetchClients,
  fetchResources,
  fetchUnits,
  createShipmentDocument,
  fetchFilteredShipmentDocuments,
  signShipmentDocument,
  revokeShipmentDocument,
  deleteShipmentDocument,
  deleteShipmentResourceFromDocument,
  updateShipmentDocument
} from '../services/api';

const ShipmentPage = () => {
  const [clients, setClients] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [shipmentDocuments, setShipmentDocuments] = useState([]);
  const [errorMessage, setErrorMessage] = useState('');

  const [filters, setFilters] = useState({
    fromDate: '',
    toDate: '',
    numbers: [],
    resourceIds: [],
    unitOfMeasurementIds: [],
  });

  const [shipmentDocument, setShipmentDocument] = useState({
    number: '',
    date: new Date().toISOString(),
    clientId: '',
    shipmentResources: [{ resourceId: '', unitOfMeasurementId: '', quantity: 0 }],
  });

  const [isEditing, setIsEditing] = useState(false);
  const [editingDocument, setEditingDocument] = useState(null);

  
  const loadData = async () => {
    try {
      const [clientsResp, resourcesResp, unitsResp] = await Promise.all([
        fetchClients(),
        fetchResources(),
        fetchUnits(),
      ]);
      setClients(clientsResp.data || []);
      setResources(resourcesResp.data || []);
      setUnits(unitsResp.data || []);
    } catch (error) {
      console.error('Ошибка при загрузке данных:', error);
    }
  };

  const loadShipmentDocuments = async () => {
    try {
      const response = await fetchFilteredShipmentDocuments(filters);
      setShipmentDocuments(response || []);
    } catch (error) {
      console.error('Ошибка при загрузке документов:', error);
    }
  };

  useEffect(() => {
    loadData();
    loadShipmentDocuments();
  }, []);

  const handleFilterChange = (selectedOptions, name) => {
    const selectedValues = selectedOptions ? selectedOptions.map(opt => opt.value) : [];
    setFilters(prev => ({ ...prev, [name]: selectedValues }));
  };

  const applyFilters = () => loadShipmentDocuments();
  const resetFilters = () => {
    setFilters({ fromDate: '', toDate: '', numbers: [], resourceIds: [], unitOfMeasurementIds: [] });
    loadShipmentDocuments();
  };

  
  const handleDocumentChange = (e) => {
    const { name, value } = e.target;
    setShipmentDocument(prev => ({ ...prev, [name]: name === 'date' ? new Date(value).toISOString() : value }));
  };

  const handleInputChange = (e, index) => {
    const { name, value } = e.target;
    const updated = [...shipmentDocument.shipmentResources];
    updated[index][name] = name === 'quantity' ? Number(value) : value;
    setShipmentDocument(prev => ({ ...prev, shipmentResources: updated }));
  };

  const handleResourceChange = (selectedOption, index, fieldName) => {
    const updated = [...shipmentDocument.shipmentResources];
    updated[index][fieldName] = selectedOption ? selectedOption.value : '';
    setShipmentDocument(prev => ({ ...prev, shipmentResources: updated }));
  };

  const addResource = () => {
    setShipmentDocument(prev => ({
      ...prev,
      shipmentResources: [
        ...prev.shipmentResources,
        { resourceId: '', unitOfMeasurementId: '', quantity: 0 },
      ],
    }));
  };

  const deleteResource = (index) => {
    const updated = [...shipmentDocument.shipmentResources];
    updated.splice(index, 1);
    setShipmentDocument(prev => ({ ...prev, shipmentResources: updated }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await createShipmentDocument(shipmentDocument);
      loadShipmentDocuments();
      setShipmentDocument({
        number: '',
        date: new Date().toISOString(),
        clientId: '',
        shipmentResources: [{ resourceId: '', unitOfMeasurementId: '', quantity: 0 }],
      });
    } catch (error) {
      console.error(error);
    }
  };

  const handleSign = async (id) => {
    try {
      await signShipmentDocument(id);
      loadShipmentDocuments();
    } catch (error) {
      setErrorMessage('Документ отгрузки подписать нельзя, так как не хватает ресурсов');
      setTimeout(() => setErrorMessage(''), 5000);
    }
  };

  const handleRevoke = async (id) => { await revokeShipmentDocument(id); loadShipmentDocuments(); };
  const handleDeleteDocument = async (id) => { await deleteShipmentDocument(id); loadShipmentDocuments(); };
  const handleDeleteResourceFromDoc = async (documentId, resourceId) => {
    try {
      await deleteShipmentResourceFromDocument(documentId, resourceId);
      loadShipmentDocuments();
    } catch (error) {
      console.error('Ошибка при удалении ресурса из документа:', error);
    }
  };

  const handleEdit = (docId) => {
    const docToEdit = shipmentDocuments.find(doc => doc.id === docId);
    if (docToEdit) {
      setEditingDocument(JSON.parse(JSON.stringify(docToEdit))); 
      setIsEditing(true);
    }
  };

  const handleSaveEdit = async () => {
    try {
      await updateShipmentDocument(editingDocument.id, editingDocument);
      loadShipmentDocuments();
      setIsEditing(false);
      setEditingDocument(null);
    } catch (error) {
      console.error('Ошибка при обновлении документа:', error);
    }
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setEditingDocument(null);
  };

  const resourceOptions = resources.map(r => ({ value: r.id, label: r.name }));
  const unitOptions = units.map(u => ({ value: u.id, label: u.name }));
  const clientOptions = clients.map(c => ({ value: c.id, label: c.name }));
  const numberOptions = shipmentDocuments.map(d => ({ value: d.number, label: d.number }));

  const customStyles = { control: base => ({ ...base, width: '250px', minWidth: '200px' }), menu: base => ({ ...base, maxHeight: '200px', overflowY: 'auto' }) };
  const formatDate = (dateString) => new Date(dateString).toLocaleString();

  return (
    <div className="shipment-page-container">
      <h1>Документ отгрузки</h1>

      {errorMessage && (
        <div style={{ backgroundColor:'#f44336', color:'white', padding:'10px', marginBottom:'10px', borderRadius:'4px', textAlign:'center' }}>
          {errorMessage}
        </div>
      )}

      <h3>Фильтрация</h3>
      <div>
        <label>Дата с:</label>
        <input type="date" value={filters.fromDate} onChange={e => setFilters(prev => ({ ...prev, fromDate: e.target.value }))}/>
      </div>
      <div>
        <label>Дата по:</label>
        <input type="date" value={filters.toDate} onChange={e => setFilters(prev => ({ ...prev, toDate: e.target.value }))}/>
      </div>
      <div>
        <label>Номера документов:</label>
        <Select
          isMulti
          value={numberOptions.filter(opt => filters.numbers.includes(opt.value))}
          onChange={selected => handleFilterChange(selected, 'numbers')}
          options={numberOptions}
          placeholder="Выберите номера"
          styles={customStyles}
        />
      </div>
      <div>
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
      <div>
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
      <div style={{ marginTop: '10px' }}>
        <button onClick={applyFilters} style={{ marginRight: '10px' }}>Применить фильтрацию</button>
        <button onClick={resetFilters}>Сбросить фильтрацию</button>
      </div>

      {isEditing && editingDocument && (
        <div style={{ marginTop: '20px', border: '1px solid #ccc', padding: '15px', borderRadius: '5px' }}>
          <h3>Редактирование документа отгрузки</h3>
          <form onSubmit={(e) => { e.preventDefault(); handleSaveEdit(); }}>
            <div>
              <label>Номер:</label>
              <input 
                type="text" 
                value={editingDocument.number} 
                onChange={(e) => setEditingDocument({...editingDocument, number: e.target.value})} 
                required
              />
            </div>
            <div>
              <label>Дата:</label>
              <input 
                type="datetime-local" 
                value={editingDocument.date.slice(0, 16)} 
                onChange={(e) => setEditingDocument({...editingDocument, date: new Date(e.target.value).toISOString()})} 
                required
              />
            </div>
            <div>
              <label>Клиент:</label>
              <Select
                value={clientOptions.find(opt => opt.value === editingDocument.clientId) || null}
                onChange={(sel) => setEditingDocument({...editingDocument, clientId: sel?.value || ''})}
                options={clientOptions}
                placeholder="Выберите клиента"
                styles={customStyles}
              />
            </div>

            {editingDocument.shipmentResources.map((res, idx) => (
              <div key={idx} style={{ marginBottom: '10px' }}>
                <label>Ресурс:</label>
                <Select
                  value={resourceOptions.find(opt => opt.value === res.resourceId) || null}
                  onChange={(sel) => {
                    const updatedResources = [...editingDocument.shipmentResources];
                    updatedResources[idx].resourceId = sel?.value || '';
                    setEditingDocument({...editingDocument, shipmentResources: updatedResources});
                  }}
                  options={resourceOptions}
                  placeholder="Выберите ресурс"
                  styles={customStyles}
                />
                <label>Единица измерения:</label>
                <Select
                  value={unitOptions.find(opt => opt.value === res.unitOfMeasurementId) || null}
                  onChange={(sel) => {
                    const updatedResources = [...editingDocument.shipmentResources];
                    updatedResources[idx].unitOfMeasurementId = sel?.value || '';
                    setEditingDocument({...editingDocument, shipmentResources: updatedResources});
                  }}
                  options={unitOptions}
                  placeholder="Выберите единицу измерения"
                  styles={customStyles}
                />
                <label>Количество:</label>
                <input
                  type="number"
                  value={res.quantity}
                  onChange={(e) => {
                    const updatedResources = [...editingDocument.shipmentResources];
                    updatedResources[idx].quantity = Number(e.target.value);
                    setEditingDocument({...editingDocument, shipmentResources: updatedResources});
                  }}
                  required
                />
                <button 
                  type="button" 
                  onClick={() => {
                    const updatedResources = [...editingDocument.shipmentResources];
                    updatedResources.splice(idx, 1);
                    setEditingDocument({...editingDocument, shipmentResources: updatedResources});
                  }}
                  style={{ marginLeft: '10px' }}
                >
                  Удалить ресурс
                </button>
              </div>
            ))}

            <div style={{ marginTop: '10px' }}>
              <button 
                type="button" 
                onClick={() => {
                  setEditingDocument({
                    ...editingDocument,
                    shipmentResources: [
                      ...editingDocument.shipmentResources,
                      { resourceId: '', unitOfMeasurementId: '', quantity: 0 }
                    ]
                  });
                }}
                style={{ marginRight: '10px' }}
              >
                Добавить ресурс
              </button>
              <button type="submit" style={{ marginRight: '10px' }}>Сохранить изменения</button>
              <button type="button" onClick={handleCancelEdit}>Отменить</button>
            </div>
          </form>
        </div>
      )}

      {!isEditing && (
        <>
          <h3>Создание документа отгрузки</h3>
          <form onSubmit={handleSubmit}>
            <div>
              <label>Номер:</label>
              <input type="text" name="number" value={shipmentDocument.number} onChange={handleDocumentChange} required/>
            </div>
            <div>
              <label>Дата:</label>
              <input type="datetime-local" name="date" value={shipmentDocument.date.slice(0,16)} onChange={handleDocumentChange} required/>
            </div>
            <div>
              <label>Клиент:</label>
              <Select
                value={clientOptions.find(opt => opt.value === shipmentDocument.clientId) || null}
                onChange={sel => setShipmentDocument(prev => ({ ...prev, clientId: sel?.value || '' }))} 
                options={clientOptions}
                placeholder="Выберите клиента"
                styles={customStyles}
              />
            </div>

            {shipmentDocument.shipmentResources.map((res, idx) => (
              <div key={idx} style={{ marginBottom:'10px' }}>
                <label>Ресурс:</label>
                <Select
                  value={resourceOptions.find(opt => opt.value === res.resourceId) || null}
                  onChange={sel => handleResourceChange(sel, idx, 'resourceId')}
                  options={resourceOptions}
                  placeholder="Выберите ресурс"
                  styles={customStyles}
                />
                <label>Единица измерения:</label>
                <Select
                  value={unitOptions.find(opt => opt.value === res.unitOfMeasurementId) || null}
                  onChange={sel => handleResourceChange(sel, idx, 'unitOfMeasurementId')}
                  options={unitOptions}
                  placeholder="Выберите единицу измерения"
                  styles={customStyles}
                />
                <label>Количество:</label>
                <input
                  type="number"
                  name="quantity"
                  value={res.quantity}
                  onChange={e => handleInputChange(e, idx)}
                  required
                />
              </div>
            ))}

            <div style={{ marginTop:'10px' }}>
              <button type="button" onClick={addResource} style={{ marginRight:'10px' }}>Добавить ресурс</button>
              <button type="button" onClick={() => deleteResource(shipmentDocument.shipmentResources.length - 1)} style={{ marginRight:'10px' }}>Удалить ресурс</button>
              <button type="submit">Создать документ</button>
            </div>
          </form>
        </>
      )}

      {!isEditing && (
        <>
          <h3>Список документов отгрузки</h3>
          <table>
            <thead>
              <tr>
                <th>Номер</th>
                <th>Дата</th>
                <th>Клиент</th>
                <th>Ресурс</th>
                <th>Единица измерения</th>
                <th>Количество</th>
                <th>Удалить ресурс</th>
                <th>Состояние</th>
                <th>Действие</th>
              </tr>
            </thead>
            <tbody>
              {shipmentDocuments.length > 0 ? shipmentDocuments.map(doc => (
                <React.Fragment key={doc.id}>
                  {doc.shipmentResources.map((res, idx) => (
                    <tr key={idx}>
                      {idx === 0 && (
                        <>
                          <td rowSpan={doc.shipmentResources.length}>{doc.number}</td>
                          <td rowSpan={doc.shipmentResources.length}>{formatDate(doc.date)}</td>
                          <td rowSpan={doc.shipmentResources.length}>{clients.find(c => c.id === doc.clientId)?.name}</td>
                        </>
                      )}
                      <td>{resources.find(r => r.id === res.resourceId)?.name}</td>
                      <td>{units.find(u => u.id === res.unitOfMeasurementId)?.name}</td>
                      <td>{res.quantity}</td>
                      <td>
                        <button onClick={() => handleDeleteResourceFromDoc(doc.id, res.resourceId)}>Удалить ресурс</button>
                      </td>
                      {idx === 0 && (
                        <td rowSpan={doc.shipmentResources.length}>
                          {!doc.state && <button onClick={() => handleSign(doc.id)}>Подписать</button>}
                          {doc.state && <button onClick={() => handleRevoke(doc.id)}>Отозвать</button>}
                        </td>
                      )}
                      {idx === 0 && (
                        <td rowSpan={doc.shipmentResources.length}>
                          <button onClick={() => handleEdit(doc.id)}>Изменить</button>
                        </td>
                      )}
                    </tr>
                  ))}
                </React.Fragment>
              )) : (
                <tr><td colSpan="10">Нет данных для отображения</td></tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
};

export default ShipmentPage;