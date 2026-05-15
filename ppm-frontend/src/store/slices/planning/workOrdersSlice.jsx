import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

const LOADING_DEFAULT = false;
const initialState = {
  items: [],
  loading: LOADING_DEFAULT,
  error: null,
};

//загрузка данных пользователя
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

const workOrdersSlice = createSlice({
  name: 'workOrders',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    //getOrders
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
    });
    builder.addCase(getOrders.rejected, (state, action) => {
      state.loading = false;
      state.error = action.payload;
    });   
  },
});

export default workOrdersSlice.reducer;