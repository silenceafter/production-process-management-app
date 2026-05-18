import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

const initialState = {
  items: [],
  loading: false,
  error: null,
  hasMore: true,
  search: '',
  limit: 500,
  page: 1,
};

export const getProducts = createAsyncThunk(
  'productsList/getProducts',
  async ({ search, limit, page }, { rejectWithValue }) => {
    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      const projectPath = import.meta.env.VITE_API_PROJECT_PATH;
      const response = await fetch('/api/Products');
      const data = await response.json();
      if (!response.ok) {
        throw new Error(data.message || 'Network response was not ok');
      }
      return data;
    } catch (error) {
      return rejectWithValue(error.message);
    }
  }
);

const productsListSlice = createSlice({
  name: 'productsList',
  initialState,
  extraReducers: (builder) => {
    builder
      .addCase(getProducts.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getProducts.fulfilled, (state, action) => {
        state.loading = false;
        const newItems = Object.values(action.payload);
        const merged = [...state.items, ...newItems];
            
        // дубликаты по полю id
        state.items = Array.from(
            new Map(merged.map(item => [item.id, item])).values()
        );
      })
      .addCase(getProducts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
      });
  },
});

export default productsListSlice.reducer;