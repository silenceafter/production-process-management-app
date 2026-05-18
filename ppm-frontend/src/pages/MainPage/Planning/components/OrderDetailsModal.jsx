import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
  Button,
  Typography,
  Box,
  Chip,
  Divider,
  TextField,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TableContainer,
  Paper,
  ToggleButtonGroup,
  ToggleButton,
  IconButton,
  Tooltip, Stack
} from '@mui/material';
import {
  AccessTime,
  Warning,
  Shield,
  Timeline,
  CheckCircle,
  ErrorOutline,
  Refresh,
  Close,
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import { DateTimePicker } from '@mui/x-date-pickers';
import PertAnalysisCard from './PertCardAnalysis';
import { 
  calculatePert
} from '../../../../store/slices/planning/workOrdersSlice';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
dayjs.extend(utc); 

// Стилизованные ячейки таблицы (как в Planning)
const StyledTableCell = styled(TableCell)(({ theme }) => ({
  '&.MuiTableCell-head': {
    backgroundColor: 'rgb(8, 22, 39)',
    color: theme.palette.common.white,
    fontWeight: 600,
  },
}));

const StyledTableRow = styled(TableRow)(({ theme }) => ({
  '&:nth-of-type(odd)': {
    backgroundColor: theme.palette.action.hover,
  },
}));

// Компонент модального окна (в стиле CreateOrderModal)
export default function OrderDetailsModal({ 
  open, 
  onClose, 
  orderId, 
  orderData,
  loading: externalLoading = false 
}) {
  const dispatch = useDispatch();

  // стейты
  const [scenario, setScenario] = useState('expected');
  const [pertData, setPertData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [tabValue, setTabValue] = useState(0);
  const [orderNumber, setOrderNumber] = useState(orderData?.order_number || '');

  const staticPertData = {
  "workOrderId": "39c17c3a-5dd5-4c2f-a2d0-e7b01c5f4bb0",
  "orderNumber": "WO-26-00032",
  "scenario": "pessimistic",
  "totalDuration": 429,
  "criticalCount": 5,
  "operations": [
    {
      "id": "dc3fc74c-8126-4982-9924-e9e6a47389c6",
      "operationCode": "005",
      "durationExpected": 39,
      "earliestStart": 0,
      "latestStart": 0,
      "slack": 0,
      "isCriticalPath": true
    },
    {
      "id": "1f2e5aa4-0f3d-4a4b-8b6d-ff612f062c70",
      "operationCode": "015",
      "durationExpected": 169,
      "earliestStart": 39,
      "latestStart": 39,
      "slack": 0,
      "isCriticalPath": true
    },
    {
      "id": "6fcfd3d2-f110-4d68-9d67-8e56700bbaaa",
      "operationCode": "012",
      "durationExpected": 104,
      "earliestStart": 208,
      "latestStart": 208,
      "slack": 0,
      "isCriticalPath": true
    },
    {
      "id": "f37e5948-5dcd-4c76-8305-623df4c2852a",
      "operationCode": "025",
      "durationExpected": 78,
      "earliestStart": 312,
      "latestStart": 312,
      "slack": 0,
      "isCriticalPath": true
    },
    {
      "id": "027218b1-2e72-444f-b659-a1a2cd1b34aa",
      "operationCode": "030",
      "durationExpected": 39,
      "earliestStart": 390,
      "latestStart": 390,
      "slack": 0,
      "isCriticalPath": true
    }
  ]
};

  // Сброс состояния при закрытии
  const handleReset = () => {
    setPertData(null);
    setError(null);
    setTabValue(0);
    onClose();
  };

  // эффекты
  useEffect(() => {
    /*if (open && orderId) {
      dispatch(calculatePert({ id: pertData.workOrderId, scenario: scenario }));
    }*/
  }, [open, orderId, scenario]);

  // Моковые данные для тестов (удалите при подключении реального API)
  const getMockPertData = () => ({
    workOrderId: orderId,
    scenario,
    totalDuration: scenario === 'optimistic' ? 280 : scenario === 'pessimistic' ? 412 : 335,
    criticalCount: 5,
    operations: [
      { id: '1', operationCode: '005', durationExpected: 30.5, earliestStart: 0, latestStart: 0, slack: 0, isCriticalPath: true },
      { id: '2', operationCode: '015', durationExpected: 132.2, earliestStart: 30.5, latestStart: 30.5, slack: 0, isCriticalPath: true },
      { id: '3', operationCode: '012', durationExpected: 81.3, earliestStart: 162.7, latestStart: 162.7, slack: 0, isCriticalPath: true },
      { id: '4', operationCode: '025', durationExpected: 61, earliestStart: 244, latestStart: 244, slack: 0, isCriticalPath: true },
      { id: '5', operationCode: '030', durationExpected: 30.5, earliestStart: 305, latestStart: 305, slack: 0, isCriticalPath: true },
    ],
  });

  const formatDuration = (minutes) => {
    if (!minutes && minutes !== 0) return '—';
    const hours = Math.floor(minutes / 60);
    const mins = Math.round(minutes % 60);
    return hours > 0 ? `${hours}ч ${mins}мин` : `${mins}мин`;
  };

  const scenarioConfig = {
    optimistic: { label: 'Оптимистичный', color: 'success', icon: '' },
    expected: { label: 'Ожидаемый', color: 'primary', icon: '' },
    pessimistic: { label: 'Пессимистичный', color: 'error', icon: '' },
  };

  const handleTabChange = (event, newValue) => setTabValue(newValue);
  
  const handleScenarioChange = (event, newScenario) => {
    if (newScenario) setScenario(newScenario);
  };

  // Общий индикатор загрузки
  const isLoading = loading || externalLoading;

  return (
    <>
        {orderData?.order_number && (
            <Dialog open={open} onClose={handleReset} maxWidth="sm" fullWidth>
                <DialogTitle>Заказ "{orderData?.order_number}"</DialogTitle>      
                <DialogContent dividers sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
                <PertAnalysisCard
                        pertData={staticPertData}
                        onRecalculate={() => console.log('Пересчитать PERT')}
                        onExport={() => console.log('Export to Excel')}
                    />
                </DialogContent>
                <DialogActions sx={{ p: 2 }}>
                <Button onClick={handleReset} disabled={loading}>Отмена</Button>    
                <Button 
                    /*onClick={handleSubmit} */
                    variant="contained" 
                    disabled={loading}
                    startIcon={loading && <CircularProgress size={16} color="inherit" />}
                >
                    Рассчитать
                </Button>            
                </DialogActions>
            </Dialog>
        )}
    </>);
}