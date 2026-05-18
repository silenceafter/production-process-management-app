import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

const LOADING_DEFAULT = false;
const initialState = {
  items: [],
  loading: LOADING_DEFAULT,
  error: null,
  total: 0,
  setOrderLoading: false,
  setOrderError: null,
  setOrderResponse: null,
  deleteOrderLoading: false,
  deleteOrderError: null,
  calculatePertLoading: false,
  calculatePertError: null,
  calculatePertResponse: null,
};

//загрузка заказов в системе
export const getOrders = createAsyncThunk(
  'workOrders/getOrders',
  async ({}, { getState, rejectWithValue }) => {
    try {
        const state = getState();
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        const projectPath = import.meta.env.VITE_API_PROJECT_PATH;
        //
        const response = await fetch(`/api/workOrders`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
          credentials: 'include'
        });
        
        const data = await response.json();
        if (!response.ok || (data.code && data.code >= 400)) {
          // Создаем объект ошибки с подробной информацией
          const error = {
            message: data.message || 'Ошибка сохранения данных',
            code: data.code || response.status,
            detailedErrors: data.detailedErrors || [],
            response: data.response || []
          };        
          throw new Error(error.message);
        }
        return data;
    } catch(error) {
      return rejectWithValue({
        message: error.message,
        code: error.code || 500,
        detailedErrors: error.detailedErrors || []
      });
    }        
  }
);

// добавить заказ
export const setOrder = createAsyncThunk(
  'workOrders/setOrder',
  async (payload, { getState, rejectWithValue }) => {
    try {
        const state = getState();
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        const projectPath = import.meta.env.VITE_API_PROJECT_PATH;
        //
        const response = await fetch(`/api/workOrders`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
          credentials: 'include'
        });
        
        const data = await response.json();
        if (!response.ok || (data.code && data.code >= 400)) {
          // Создаем объект ошибки с подробной информацией
          const error = {
            message: data.message || 'Ошибка сохранения данных',
            code: data.code || response.status,
            detailedErrors: data.detailedErrors || [],
            response: data.response || []
          };        
          throw new Error(error.message);
        }
        return data;
    } catch(error) {
      return rejectWithValue({
        message: error.message,
        code: error.code || 500,
        detailedErrors: error.detailedErrors || []
      });
    }        
  }
);

export const updateOrder = createAsyncThunk(
  'workOrders/updateOrder',
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL || '';
      
      const response = await fetch(`${baseUrl}/api/workOrders/${id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Ошибка ${response.status}`);
      }

      return await response.json(); // Обновлённый объект
    } catch (error) {
      return rejectWithValue({
        message: error.message || 'Не удалось обновить заказ',
        code: error.code || 500,
        detailedErrors: error.detailedErrors || []
      });
    }
  }
);

// удалить заказ
export const deleteOrder = createAsyncThunk(
  'workOrders/deleteOrder',
  async (payload, { getState, rejectWithValue }) => {
    try {
        const state = getState();
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        const projectPath = import.meta.env.VITE_API_PROJECT_PATH;
        //
        const response = await fetch(`/api/workOrders/${payload}`, {
          method: 'DELETE',
        });
        
        if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Ошибка ${response.status}`);
      }
      const data = response.status === 204 ? null : await response.json();
      
      return data || { success: true, id: payload };
    } catch(error) {
      return rejectWithValue({
        message: error.message,
        code: error.code || 500,
        detailedErrors: error.detailedErrors || []
      });
    }        
  }
);

// рассчитать сроки (PERT)
export const calculatePert = createAsyncThunk(
  'workOrders/calculatePert',
  async (payload, { getState, rejectWithValue }) => {
    try {
      const response = await fetch(`/api/workOrders/${payload.id}/calculate-pert`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
        credentials: 'include'
      });
        
      const data = await response.json();
      if (!response.ok || (data.code && data.code >= 400)) {
        // Создаем объект ошибки с подробной информацией
        const error = {
          message: data.message || 'Ошибка сохранения данных',
          code: data.code || response.status,
          detailedErrors: data.detailedErrors || [],
          response: data.response || []
        };        
        throw new Error(error.message);
      }
      return data;
    } catch(error) {
      return rejectWithValue({
        message: error.message,
        code: error.code || 500,
        detailedErrors: error.detailedErrors || []
      });
    }        
  }
);

const workOrdersSlice = createSlice({
  name: 'workOrders',
  initialState,
  reducers: {
    /*setItem: (state, action) => {
      state.orders.unshift(action.payload);
      state.currentOrder = action.payload;
      state.loading = false;
      state.success = true;
    },
    setLoading: (state, action) => {
      state.loading = action.payload;
    },
    setError: (state, action) => {
      state.error = action.payload;
      state.loading = false;
    },*/
  },
  extraReducers: (builder) => {
    // getOrders
    builder.addCase(getOrders.pending, (state) => {
      state.loading = true;
      state.error = null;
      state.items = [];
    });
    builder.addCase(getOrders.fulfilled, (state, action) => {
      state.loading = false;
      const newItems = Object.values(action.payload);
      const merged = [...state.items, ...newItems];
        
      // дубликаты по полю id
      state.items = Array.from(
        new Map(merged.map(item => [item.id, item])).values()
      );   
      state.total = action.payload.total;      
    });
    builder.addCase(getOrders.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload;
    });
    
    // setOrder
    builder.addCase(setOrder.pending, (state) => {
      state.setOrderLoading = true;
      state.setOrderError = null;
    });
    builder.addCase(setOrder.fulfilled, (state, action) => {
      state.setOrderLoading = false;
      state.setOrderResponse = action.payload;     
    });
    builder.addCase(setOrder.rejected, (state, action) => {
      state.setOrderLoading = false;
      state.setOrderError = action.payload;
    });

    // updateOrder
    builder.addCase(updateOrder.pending, (state) => {
      state.setOrderLoading = true;  // Используем тот же индикатор загрузки
      state.setOrderError = null;
    });
    builder.addCase(updateOrder.fulfilled, (state, action) => {
      state.setOrderLoading = false;
      // 👇 Точечно обновляем запись в массиве
      const index = state.items.findIndex(item => item.id === action.payload.id);
      if (index !== -1) {
        state.items[index] = { ...state.items[index], ...action.payload };
      }
    });
    builder.addCase(updateOrder.rejected, (state, action) => {
      state.setOrderLoading = false;
      state.setOrderError = action.payload;
    });

    // deleteOrder
    builder.addCase(deleteOrder.pending, (state) => {
      state.deleteOrderLoading = true;
    });
    builder.addCase(deleteOrder.fulfilled, (state, action) => {
      state.deleteOrderLoading = false;
      state.items = state.items.filter(item => item.id !== action.payload);
      //state.total = Math.max(0, state.total - 1);     
    });
    builder.addCase(deleteOrder.rejected, (state, action) => {
      state.deleteOrderLoading = false;
      state.deleteOrderError = action.payload || 'Не удалось удалить';
    });

    // calculatePert
    builder.addCase(calculatePert.pending, (state) => {
      state.calculatePertLoading = true;
      state.calculatePertError = null;
    });
    builder.addCase(calculatePert.fulfilled, (state, action) => {
      state.calculatePertLoading = false;
      state.calculatePertResponse = action.payload;
    });
    builder.addCase(calculatePert.rejected, (state, action) => {
      state.calculatePertLoading = false;
      state.calculatePertError = action.payload;
    });
  },
});

/*export const {
  setItem, setLoading, setError
} = workOrdersSlice.actions;*/
export default workOrdersSlice.reducer;