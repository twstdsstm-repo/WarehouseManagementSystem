import axios from 'axios';
import qs from 'qs';

const api = axios.create({
  baseURL: 'http://localhost:5030/api',
});

//Ресурсы
export const fetchResources = () => {
  return api.get('/resources');
};
export const fetchResourceById = (id) => {
  return api.get(`/resources/${id}`);
};
export const addResource = (resource) => {
  return api.post('/resources', resource);
};
export const updateResource = (id, resource) => {
  return api.put(`/resources/${id}`, resource);
};
export const deleteResource = (id) => {
  return api.delete(`/resources/${id}`);
};
export const unarchiveResource = (id) => {
  return api.post(`/resources/${id}/unarchive`);
};
export const archiveResource = (id) => {
  return api.post(`/resources/${id}/archive`);
};

//Единицы измерения
export const fetchUnits = () => {
  return api.get('/unitsofmeasurement'); 
};
export const addUnit = (unit) => {
  return api.post('/unitsofmeasurement', unit); 
};
export const archiveUnit = (id) => {
  return api.post(`/unitsofmeasurement/${id}/archive`); 
};
export const deleteUnit = (id) => {
  return api.delete(`/unitsofmeasurement/${id}`); 
};
export const unarchiveUnit = (id) => {
  return api.post(`/unitsofmeasurement/${id}/unarchive`);
};
export const updateUnit = async (id, unit) => {
  return api.put(`/unitsofmeasurement/${id}`, unit);
};

//Клиент
export const fetchClients = () => {
  return api.get('/clients');
};
export const addClient = (client) => {
  return api.post('/clients', client);
};
export const archiveClient = (id) => {
  return api.post(`/clients/${id}/archive`);
};
export const deleteClient = (id) => {
  return api.delete(`/clients/${id}`);
};
export const unarchiveClient = (id) => {
  return api.post(`/clients/${id}/unarchive`);
};
export const updateClient = async (id, client) => {
  return api.put(`/clients/${id}`, client);
};

// Документы поступления
export const createReceiptDocument = (receiptDocument) => {
  return api.post('/receiptdocuments', receiptDocument);
};
export const deleteReceiptDocument = async (documentId) => {
  return api.delete(`/receiptdocuments/${documentId}`);
};
export const fetchFilteredReceiptDocuments = async (filters) => {
  try {
    const params = {};

    if (filters.fromDate) params.FromDate = filters.fromDate;
    if (filters.toDate) params.ToDate = filters.toDate;
    if (filters.numbers.length > 0) params.Numbers = filters.numbers;
    if (filters.resourceIds.length > 0) params.ResourceIds = filters.resourceIds;
    if (filters.unitOfMeasurementIds.length > 0) params.UnitOfMeasurementIds = filters.unitOfMeasurementIds;

    const response = await api.get('/receiptdocuments/filter', {
      params,
      paramsSerializer: params => qs.stringify(params, { arrayFormat: 'repeat' })
    });

    console.log('Fetched data:', response.data);
    return response.data;
  } catch (error) {
    console.error('Error fetching filtered receipt documents:', error);
    throw error;
  }
};

export const deleteResourceFromDocument = (documentId, resourceId) => 
  api.delete(`/receiptdocuments/${documentId}/resources/${resourceId}`);
export const updateReceiptDocument = async (id, document) => {
  try {
    const response = await api.put(`/receiptdocuments/${id}`, document);
    return response.data;
  } catch (error) {
    console.error('Ошибка при обновлении документа поступления:', error);
    throw error;
  }
};

// Документы отгрузки
export const createShipmentDocument = (shipmentDocument) => {
  return api.post('/shipmentdocuments', shipmentDocument);
};

export const fetchFilteredShipmentDocuments = async (filters) => {
  try {
    const params = {};

    if (filters.fromDate) params.FromDate = filters.fromDate;
    if (filters.toDate) params.ToDate = filters.toDate;
    if (filters.numbers && filters.numbers.length > 0) params.Numbers = filters.numbers;
    if (filters.resourceIds && filters.resourceIds.length > 0) params.ResourceIds = filters.resourceIds;
    if (filters.unitOfMeasurementIds && filters.unitOfMeasurementIds.length > 0) params.UnitOfMeasurementIds = filters.unitOfMeasurementIds;

    const response = await api.get('/shipmentdocuments/filter', {
      params,
      paramsSerializer: params => qs.stringify(params, { arrayFormat: 'repeat' })
    });

    return response.data;
  } catch (error) {
    console.error('Ошибка при фильтрации документов отгрузки:', error);
    throw error;
  }
};
export const signShipmentDocument = (documentId) => {
  return api.post(`/shipmentdocuments/${documentId}/sign`);
};
export const revokeShipmentDocument = (documentId) => {
  return api.post(`/shipmentdocuments/${documentId}/revoke`);
};
export const deleteShipmentDocument = (documentId) => {
  return api.delete(`/shipmentdocuments/${documentId}`);
};
export const deleteShipmentResourceFromDocument = async (documentId, resourceId) => {
  try {
    await api.delete(`/shipmentdocuments/${documentId}/resources/${resourceId}`);
  } catch (error) {
    console.error('Ошибка при удалении ресурса из документа отгрузки:', error);
    throw error;
  }
};
export const updateShipmentDocument = async (id, document) => {
  try {
    const response = await api.put(`/shipmentdocuments/${id}`, document);
    return response.data;
  } catch (error) {
    console.error('Ошибка при обновлении документа отгрузки:', error);
    throw error;
  }
};

//Баланс
export const fetchFilteredBalances = async (filters) => {
  try {
    const params = {};

    if (filters.resourceIds && filters.resourceIds.length > 0) {
      params.ResourceIds = filters.resourceIds;
    }

    if (filters.unitOfMeasurementIds && filters.unitOfMeasurementIds.length > 0) {
      params.UnitOfMeasurementIds = filters.unitOfMeasurementIds;
    }

    const response = await api.get('/balances/filter', {
      params,
      paramsSerializer: params => {
        // Используем qs для корректной сериализации массивов
        return require('qs').stringify(params, { arrayFormat: 'repeat' });
      }
    });

    return response.data;
  } catch (error) {
    console.error('Ошибка при фильтрации баланса:', error);
    throw error;
  }
};
export const deleteBalance = async (balanceId) => {
  try {
    await api.delete(`/balances/${balanceId}`);
  } catch (error) {
    console.error('Ошибка при удалении баланса:', error);
    throw error;
  }
};



export default api;
