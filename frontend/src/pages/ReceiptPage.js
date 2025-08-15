import React, { useState, useEffect } from 'react';
import { 
  fetchResources, 
  fetchUnits, 
  createReceiptDocument, 
  fetchFilteredReceiptDocuments, 
  deleteResourceFromDocument,
  updateReceiptDocument
} from '../services/api';
import Select from 'react-select';
import '../componets/styles.css';

const ReceiptPage = () => {
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [receiptDocuments, setReceiptDocuments] = useState([]);
  const [errorMessage, setErrorMessage] = useState('');

  const [filters, setFilters] = useState({
    fromDate: '',
    toDate: '',
    numbers: [],
    resourceIds: [],
    unitOfMeasurementIds: [],
  });

  const [receiptDocument, setReceiptDocument] = useState({
      number: '',
      date: new Date().toISOString(),
      receiptResources: [] 
  });

  const [isEditing, setIsEditing] = useState(false);
  const [editingDocument, setEditingDocument] = useState(null);

  
  const loadResourcesAndUnits = async () => {
    try {
      const [resourcesResponse, unitsResponse] = await Promise.all([
        fetchResources(),
        fetchUnits(),
      ]);
      setResources(resourcesResponse.data || []);
      setUnits(unitsResponse.data || []);
    } catch (error) {
      console.error('Error fetching resources or units:', error);
    }
  };

  const loadReceiptDocuments = async () => {
    try {
      const response = await fetchFilteredReceiptDocuments(filters);
      
      const processedDocs = response.map(doc => ({
        ...doc,
        receiptResources: doc.receiptResources || [] 
      }));
      setReceiptDocuments(processedDocs);
    } catch (error) {
      console.error('Error loading receipt documents:', error);
      setErrorMessage('Ошибка загрузки документов');
    }
  };

  useEffect(() => {
    loadResourcesAndUnits();
    loadReceiptDocuments();
  }, []);

  const handleFilterChange = (selectedOptions, name) => {
    const selectedValues = selectedOptions ? selectedOptions.map(option => option.value) : [];
    setFilters(prevFilters => ({
      ...prevFilters,
      [name]: selectedValues,
    }));
  };

  const handleApplyFilters = () => {
    loadReceiptDocuments();
  };

  const resetFilters = () => {
    setFilters({
      fromDate: '',
      toDate: '',
      numbers: [],
      resourceIds: [],
      unitOfMeasurementIds: [],
    });
    loadReceiptDocuments();
  };

  
  const handleDocumentChange = (e) => {
    const { name, value } = e.target;
    setReceiptDocument(prev => ({
      ...prev,
      [name]: name === 'date' ? new Date(value).toISOString() : value
    }));
  };

  const handleInputChange = (e, index) => {
    const { name, value } = e.target;
    const updated = [...receiptDocument.receiptResources];
    updated[index][name] = name === 'quantity' ? Number(value) : value;
    setReceiptDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  const handleResourceChange = (selectedOption, index, fieldName) => {
    const updated = [...receiptDocument.receiptResources];
    updated[index][fieldName] = selectedOption ? selectedOption.value : '';
    setReceiptDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  const handleAddResource = () => {
      setReceiptDocument(prev => ({
          ...prev,
          receiptResources: [
              ...prev.receiptResources,
              { 
                  resourceId: '', 
                  unitOfMeasurementId: '', 
                  quantity: 0 
              }
          ]
      }));
  };

  const handleDeleteResourceInForm = (index) => {
    const updated = [...receiptDocument.receiptResources];
    updated.splice(index, 1);
    setReceiptDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  const handleSubmit = async (e) => {
      e.preventDefault();
      try {
          
          const docToSend = {
              ...receiptDocument,
              receiptResources: receiptDocument.receiptResources.filter(
                  r => r.resourceId && r.unitOfMeasurementId && r.quantity > 0
              )
          };
          
          await createReceiptDocument(docToSend);
          loadReceiptDocuments();
          setReceiptDocument({
              number: '',
              date: new Date().toISOString(),
              receiptResources: []
          });
      } catch (error) {
          console.error('Error creating receipt document:', error);
          setErrorMessage(error.response?.data?.message || error.message);
      }
  };

  
  const handleDeleteResource = async (documentId, resourceId) => {
    try {
      await deleteResourceFromDocument(documentId, resourceId);
      loadReceiptDocuments();
    } catch (error) {
      console.error('Error deleting resource from document:', error);
    }
  };

  
  const handleEdit = (docId) => {
    const docToEdit = receiptDocuments.find(doc => doc.id === docId);
    if (docToEdit) {
      setEditingDocument(JSON.parse(JSON.stringify(docToEdit)));
      setIsEditing(true);
    }
  };

  const handleSaveEdit = async () => {
    try {
      await updateReceiptDocument(editingDocument.id, editingDocument);
      loadReceiptDocuments();
      setIsEditing(false);
      setEditingDocument(null);
    } catch (error) {
      console.error('Error updating document:', error);
    }
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setEditingDocument(null);
  };

  const handleEditInputChange = (e, index) => {
    const { name, value } = e.target;
    const updated = [...editingDocument.receiptResources];
    updated[index][name] = name === 'quantity' ? Number(value) : value;
    setEditingDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  const handleEditResourceChange = (selectedOption, index, fieldName) => {
    const updated = [...editingDocument.receiptResources];
    updated[index][fieldName] = selectedOption ? selectedOption.value : '';
    setEditingDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  const handleAddResourceInEdit = () => {
    setEditingDocument(prev => ({
      ...prev,
      receiptResources: [
        ...prev.receiptResources,
        { resourceId: '', unitOfMeasurementId: '', quantity: 0 }
      ]
    }));
  };

  const handleDeleteResourceInEdit = (index) => {
    const updated = [...editingDocument.receiptResources];
    updated.splice(index, 1);
    setEditingDocument(prev => ({
      ...prev,
      receiptResources: updated
    }));
  };

  
  const formatDate = (dateString) => new Date(dateString).toLocaleString('ru-RU');
  const formatDateForInput = (dateString) => new Date(dateString).toISOString().slice(0, 16);

  const resourceOptions = resources.map(resource => ({ value: resource.id, label: resource.name }));
  const unitOptions = units.map(unit => ({ value: unit.id, label: unit.name }));
  const numberOptions = receiptDocuments.map(doc => ({ value: doc.number, label: doc.number }));

  const customStyles = {
    control: (base) => ({ ...base, width: '25%', maxWidth: '300px', minWidth: '200px' }),
    menu: (base) => ({ ...base, width: '100%', maxWidth: '300px', fontSize: '14px', maxHeight: '200px', overflowY: 'auto' }),
  };

  return (
    <div className="receipt-page-container">
      <h1>Документ поступления</h1>

      {errorMessage && (
        <div style={{ backgroundColor:'#f44336', color:'white', padding:'10px', marginBottom:'10px', borderRadius:'4px', textAlign:'center' }}>
          {errorMessage}
        </div>
      )}

      <h3>Фильтрация</h3>
      <div className="filter-section">
        <div>
          <label>Дата с:</label>
          <input 
            type="date" 
            value={filters.fromDate} 
            onChange={e => setFilters(prev => ({ ...prev, fromDate: e.target.value }))} 
          />
        </div>

        <div>
          <label>Дата по:</label>
          <input 
            type="date" 
            value={filters.toDate} 
            onChange={e => setFilters(prev => ({ ...prev, toDate: e.target.value }))} 
          />
        </div>

        <div>
          <label>Номера документов:</label>
          <Select
            isMulti
            value={numberOptions.filter(option => filters.numbers.includes(option.value))}
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
            value={resourceOptions.filter(option => filters.resourceIds.includes(option.value))}
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
            value={unitOptions.filter(option => filters.unitOfMeasurementIds.includes(option.value))}
            onChange={selected => handleFilterChange(selected, 'unitOfMeasurementIds')}
            options={unitOptions}
            placeholder="Выберите единицы измерения"
            styles={customStyles}
          />
        </div>
      </div>

      <div style={{ marginTop: '10px' }}>
        <button onClick={handleApplyFilters} style={{ marginRight: '10px' }}>Применить фильтры</button>
        <button onClick={resetFilters}>Сбросить фильтры</button>
      </div>

      {isEditing && editingDocument && (
        <div style={{ marginTop: '20px', border: '1px solid #ccc', padding: '15px', borderRadius: '5px' }}>
          <h3>Редактирование документа поступления</h3>
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
                value={formatDateForInput(editingDocument.date)} 
                onChange={(e) => setEditingDocument({...editingDocument, date: new Date(e.target.value).toISOString()})} 
                required
              />
            </div>

            {editingDocument.receiptResources.map((res, idx) => (
              <div key={idx} style={{ marginBottom: '10px' }}>
                <label>Ресурс:</label>
                <Select
                  value={resourceOptions.find(opt => opt.value === res.resourceId) || null}
                  onChange={(sel) => handleEditResourceChange(sel, idx, 'resourceId')}
                  options={resourceOptions}
                  placeholder="Выберите ресурс"
                  styles={customStyles}
                />
                <label>Единица измерения:</label>
                <Select
                  value={unitOptions.find(opt => opt.value === res.unitOfMeasurementId) || null}
                  onChange={(sel) => handleEditResourceChange(sel, idx, 'unitOfMeasurementId')}
                  options={unitOptions}
                  placeholder="Выберите единицу измерения"
                  styles={customStyles}
                />
                <label>Количество:</label>
                <input
                  type="number"
                  name="quantity"
                  value={res.quantity}
                  onChange={(e) => handleEditInputChange(e, idx)}
                  required
                />
                {editingDocument.receiptResources.length > 1 && (
                  <button 
                    type="button" 
                    onClick={() => handleDeleteResourceInEdit(idx)}
                    style={{ marginLeft: '10px' }}
                  >
                    Удалить ресурс
                  </button>
                )}
              </div>
            ))}

            <div style={{ marginTop: '10px' }}>
              <button 
                type="button" 
                onClick={handleAddResourceInEdit}
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
          <h3>Создание документа поступления</h3>
          <form onSubmit={handleSubmit}>
            <div>
              <label>Номер:</label>
              <input 
                type="text" 
                name="number" 
                value={receiptDocument.number} 
                onChange={handleDocumentChange} 
                required 
              />
            </div>

            <div>
              <label>Дата:</label>
              <input 
                type="datetime-local" 
                name="date" 
                value={formatDateForInput(receiptDocument.date)} 
                onChange={handleDocumentChange} 
                required 
              />
            </div>

            {receiptDocument.receiptResources.map((resource, index) => (
              <div key={index} style={{ marginBottom: '10px' }}>
                <div>
                  <label>Ресурс:</label>
                  <Select
                    value={resourceOptions.find(opt => opt.value === resource.resourceId) || null}
                    onChange={(sel) => handleResourceChange(sel, index, 'resourceId')}
                    options={resourceOptions}
                    placeholder="Выберите ресурс"
                    styles={customStyles}
                  />
                </div>

                <div>
                  <label>Единица измерения:</label>
                  <Select
                    value={unitOptions.find(opt => opt.value === resource.unitOfMeasurementId) || null}
                    onChange={(sel) => handleResourceChange(sel, index, 'unitOfMeasurementId')}
                    options={unitOptions}
                    placeholder="Выберите единицу измерения"
                    styles={customStyles}
                  />
                </div>

                <div>
                  <label>Количество:</label>
                  <input
                    type="number"
                    name="quantity"
                    value={resource.quantity}
                    onChange={(e) => handleInputChange(e, index)}
                    required
                  />
                </div>

                {receiptDocument.receiptResources.length > 1 && (
                  <div>
                    <button 
                      type="button" 
                      onClick={() => handleDeleteResourceInForm(index)}
                    >
                      Удалить ресурс
                    </button>
                  </div>
                )}
              </div>
            ))}

            <div style={{ marginTop: '10px' }}>
              <button 
                type="button" 
                onClick={handleAddResource}
                style={{ marginRight: '10px' }}
              >
                Добавить ресурс
              </button>
              <button type="submit">Создать документ</button>
            </div>
          </form>
        </>
      )}

      {!isEditing && (
        <>
          <h3>Список документов поступления</h3>
          <table>
            <thead>
              <tr>
                <th>Номер</th>
                <th>Дата</th>
                <th>Ресурс</th>
                <th>Единица измерения</th>
                <th>Количество</th>
                <th>Удалить ресурс</th>
                <th>Действие</th>
              </tr>
            </thead>
            <tbody>
              {receiptDocuments.length > 0 ? (
                receiptDocuments.map((document) => (
                  <React.Fragment key={document.id}>
                    {document.receiptResources.length > 0 ? (
                      document.receiptResources.map((resource, index) => (
                        <tr key={`${document.id}-${index}`}>
                          {index === 0 && (
                            <>
                              <td rowSpan={document.receiptResources.length}>{document.number}</td>
                              <td rowSpan={document.receiptResources.length}>{formatDate(document.date)}</td>
                            </>
                          )}
                          <td>{resources.find(r => r.id === resource.resourceId)?.name || '-'}</td>
                          <td>{units.find(u => u.id === resource.unitOfMeasurementId)?.name || '-'}</td>
                          <td>{resource.quantity}</td>
                          <td>
                            <button onClick={() => handleDeleteResource(document.id, resource.resourceId)}>
                              Удалить ресурс
                            </button>
                          </td>
                          {index === 0 && (
                            <td rowSpan={document.receiptResources.length}>
                              <button onClick={() => handleEdit(document.id)}>Изменить</button>
                            </td>
                          )}
                        </tr>
                      ))
                    ) : (
                      
                      <tr key={document.id}>
                        <td>{document.number}</td>
                        <td>{formatDate(document.date)}</td>
                        <td colSpan="4">Нет ресурсов</td>
                        <td>
                          <button onClick={() => handleEdit(document.id)}>Изменить</button>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                ))
              ) : (
                <tr><td colSpan="7">Нет данных для отображения</td></tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
};

export default ReceiptPage;