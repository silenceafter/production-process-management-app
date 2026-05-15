import { configureStore } from '@reduxjs/toolkit';
import { persistStore, persistReducer, FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER } from 'redux-persist';
import storage from 'redux-persist/lib/storage';
import { combineReducers } from 'redux';
import { thunk } from 'redux-thunk'; // Middleware для асинхронных действий
//import autoMergeLevel2 from 'redux-persist/lib/stateReconciler/autoMergeLevel2';
import operationsListReducer from './slices/technologies/lists/operationsListSlice';
import jobsListReducer from './slices/technologies/lists/jobsListSlice';
import measuringToolsListReducer from './slices/technologies/lists/measuringToolsListSlice';
import toolingListReducer from './slices/technologies/lists/toolingListSlice';
import componentsListReducer from './slices/technologies/lists/componentsListSlice';
import materialsListReducer from './slices/technologies/lists/materialsListSlice';
import technologiesListReducer from './slices/technologies/lists/technologiesListSlice';
import equipmentListReducer from './slices/technologies/lists/equipmentListSlice';
import drawingsReducer from './slices/technologies/drawingsSlice';
import usersReducer from './slices/usersSlice';
import technologiesReducer from './slices/technologies/technologiesSlice';
import unsavedChangesReducer from './slices/unsavedChangesSlice';
import technologiesPrefixReducer from './slices/technologies/technologiesPrefixSlice';
import dashboardReducer from './slices/dashboardSlice';
import productsReducer from './slices/technologies/productsSlice';
import adminReducer from './slices/adminSlice';
import workOrdersReducer from './slices/planning/workOrdersSlice';

//корневой редьюсер
const rootReducer = combineReducers({  
  operationsList: operationsListReducer,
  jobsList: jobsListReducer,
  measuringToolsList: measuringToolsListReducer,
  toolingList: toolingListReducer /* оснастка */,
  componentsList: componentsListReducer,
  materialsList: materialsListReducer,
  equipmentList: equipmentListReducer,
  unsavedChanges: unsavedChangesReducer,
  drawings: drawingsReducer,
  products: productsReducer,
  technologies: technologiesReducer,
  technologiesPrefix: technologiesPrefixReducer,
  users: usersReducer, /* храним обязательно */  
  dashboard: dashboardReducer,
  admin: adminReducer,
  workOrders: workOrdersReducer,
});

//конфигурация persist
const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['users', 'dashboard' ],
  blacklist: ['technologies', 'technologiesPrefix', 'products', 'drawings' ],
  /*stateReconciler: autoMergeLevel2,*/
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

//хранилище redux
/*rootReducer,*/
/*const store = configureStore({
  reducer: persistedReducer, 
  middleware: (getDefaultMiddleware) => getDefaultMiddleware({serializableCheck:false}).concat(thunk),
});*/

const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
      },
    })
});

const persistor = persistStore(store);
export { store, persistor };