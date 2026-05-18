import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { 
  Accordion, 
  AccordionActions, 
  AccordionSummary, 
  AccordionDetails, 
  AppBar, 
  Backdrop,
  Box, 
  Button, 
  ButtonGroup, 
  Card,CardContent,
  Chip,
  CircularProgress, 
  Grid, 
  IconButton, 
  InputAdornment,
  Link,
  Paper, 
  Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TablePagination,
  TextField,
  Typography,
  Stack,
  rgbToHex
} from '@mui/material';
import {
  Assessment,
  Inventory,
  Schedule,
  Warning,
  Security,
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import LoadingButton from '@mui/lab/LoadingButton';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SearchIcon from '@mui/icons-material/Search';
import AnalyticsIcon from '@mui/icons-material/Analytics';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useSnackbar } from 'notistack';
//import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import CreateOrderModal from './components/CreateOrderModal';
import OrderDetailsModal from './components/OrderDetailsModal';
import { 
  getOrders as workOrdersFetchData,
  setOrder,
  updateOrder,
  deleteOrder,
  /*setOrder as workOrderSave,
  setLoading as workOrderLoading,
  setError as workOrderError*/
} from '../../../store/slices/planning/workOrdersSlice';
import { getProducts as productsListFetchData } from '../../../store/slices/technologies/lists/productsListSlice';
import dayjs from 'dayjs';

function Planning({ showLoading }) {
  const dispatch = useDispatch();
  const StyledTableCell = styled(TableCell)(({ theme }) => ({
    '&.MuiTableCell-head': { // селектор для заголовочной клетки
      backgroundColor: 'rgb(8, 22, 39)',
      color: theme.palette.common.white,
      padding: '8px 12px',
    },
    '&.MuiTableCell-body': { // селектор для основной клетки
      fontSize: 14,
      padding: '8px 12px',      
      verticalAlign: 'middle',
    },
  }));
  const StyledTableRow = styled(TableRow)(({ theme }) => ({
    '&:nth-of-type(odd)': {
      backgroundColor: theme.palette.action.hover,
    },
    '&:last-child td, &:last-child th': {
      border: 0,
    },
  }));
  const planningWorkOrdersHeaders = [
    { key: 'row_num', label: '#', type: 'number', align: 'center' },
    { key: 'order_number', label: 'Номер заказа', type: 'text', align: 'left' },
    { key: 'drawing_number', label: 'Наименование изделия', type: 'text', align: 'left' },
    { key: 'product_name', label: 'Номер изделия', type: 'text', align: 'left' },
    { key: 'quantity', label: 'Количество', type: 'number', align: 'center' },
    { key: 'planned_start', label: 'Приступить', type: 'date', align: 'center' },
    { key: 'due_date', label: 'Выполнить', type: 'date', align: 'center' },
    { key: 'status', label: 'Статус', type: 'status', align: 'left' },
    { key: 'created_at', label: 'Дата добавления', type: 'date', align: 'center' },
    { key: 'actions', label: 'Действия', type: 'action', align: 'center' },
  ];
  const statusConfig = {
    'Draft': { label: 'Черновик', color: 'primary' },
    'Released': { label: 'Активен', color: 'info' },
    'InProgress': { label: 'Завершён', color: 'success' },
    'Completed': { label: 'Отменён', color: 'error' },
    'Cancelled': { label: 'Ожидание', color: 'warning' },    
  };

  // информационные карточки
  const cards = [
  {
    title: 'ПЛАНИРОВАНИЕ ПЕРИОДА',
    isHeader: true,
  },
  {
    title: 'Всего заказов',
    value: '7',
    subtitle: '(5 активных)',
    icon: <Inventory sx={{ fontSize: 40, color: '#1976d2' }} />,
    color: '#1976d2',
  },
  {
    title: 'Ожидаемо',
    value: '38.5 часов',
    subtitle: '(по PERT)',
    icon: <Schedule sx={{ fontSize: 40, color: '#2e7d32' }} />,
    color: '#2e7d32',
  },
  {
    title: 'Риск',
    value: '+12.3 часа',
    subtitle: '(пессим.)',
    icon: <Warning sx={{ fontSize: 40, color: '#ed6c02' }} />,
    color: '#ed6c02',
  },
  {
    title: 'Буфер',
    value: '8.2 часа',
    subtitle: '(остаток)',
    icon: <Security sx={{ fontSize: 40, color: '#9c27b0' }} />,
    color: '#9c27b0',
  },
];

  const formatDate = (value) => {
    if (!value) return '—';
    return dayjs(value).locale('ru').format('DD.MM.YYYY');
  };
  const formatDateTime = (value) => {
    if (!value) return '—';
    return dayjs(value).locale('ru').format('DD.MM.YYYY HH:mm');
  };

  //стейты
  const [isSaving, setIsSaving] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editModalData, setEditModalData] = useState(null);
  
  // пагинация (пока не работает)
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(50);
  
  // строка таблицы
  const [selectedRowId, setSelectedRowId] = useState(null);

  // детализация заказа
  const [detailsModalOpen, setDetailsModalOpen] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState(null);

  //селекторы
  const workOrdersItems = useSelector((state) => state.planning.workOrders.items);
  const workOrdersLoading = useSelector((state) => state.planning.workOrders.loading);
  const workOrdersTotal = useSelector((state) => state.planning.workOrders.total);
  const setOrderLoading = useSelector((state) => state.planning.setOrderLoading);
  const productsListItems = useSelector((state) => state.productsList.items);
  const productsListLoading = useSelector((state) => state.productsList.loading);

  //рефы
  //хуки
  const { enqueueSnackbar } = useSnackbar();

  //события
  const handleCreateOrder = async (newOrderData) => {
    try {
      await dispatch(setOrder(newOrderData)).unwrap();
      await dispatch(workOrdersFetchData({}));      
      setIsModalOpen(false);
      enqueueSnackbar('Заказ успешно добавлен', { variant: 'success' });
    } catch (error) {
      enqueueSnackbar(`Ошибка: ${error.message}`, { variant: 'error' });
    }
  };

  const updateOrderClick = (selectedRowId) => {
    const row = workOrdersItems.find((item) => item.id === selectedRowId);
    setEditModalData(row);
    setIsModalOpen(true);
};

// Сохранить изменения
const handleSaveOrder = async (payload) => {
  try {
    if (editModalData?.id) {
      await dispatch(updateOrder({ 
        id: editModalData.id, 
        data: payload 
      })).unwrap();
      enqueueSnackbar('Заказ обновлён', { variant: 'success' });
    } else {
      await dispatch(setOrder(payload)).unwrap();
      enqueueSnackbar('Заказ создан', { variant: 'success' });
    }
    //
    setIsModalOpen(false); //setEditModalOpen(false);
    setEditModalData(null);
    setSelectedRowId(null);
    dispatch(workOrdersFetchData({ page: page + 1, pageSize }));
  } catch (error) {
    enqueueSnackbar(error.message || 'Ошибка сохранения', { variant: 'error' });
  }
};

const deleteOrderClick = async (selectedRowId) => {
    try {
      await dispatch(deleteOrder(selectedRowId)).unwrap();
      await dispatch(workOrdersFetchData({}));
      enqueueSnackbar('Заказ удален', { variant: 'success' });
    } catch (error) {
      enqueueSnackbar(`Ошибка: ${error.message}`, { variant: 'error' });
    }
  }

const showOrderDetails = (selectedRowId) => {
  const row = workOrdersItems.find((item) => item.id === selectedRowId);
  setSelectedOrder(row);
  setDetailsModalOpen(true);
};

  //эффекты
  useEffect(() => {
    dispatch(workOrdersFetchData({
      page: page + 1,
      pageSize: pageSize,
    }));
    dispatch(productsListFetchData({}));
}, [page, pageSize, dispatch]);

  // вывод
  return (
    <>      
      <Box sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
        height: '100%',
        width: '100%', /*27*/
        gap: 2,
      }}>        
        {/* Фильтры и кнопки */}
        <Paper sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'stretch',
          /*height: '8%',*/
          width: '100%',
          padding: 2,
          gap: 2
        }}>
            <Grid item>
              <Typography variant="h6" component="div" noWrap fontWeight={500}>Производственные заказы</Typography>
            </Grid>
            <Grid container spacing={2} alignItems="flex-end">
              <Grid item xs={4}>
                  <Stack spacing={1} sx={{ width: '100%' }}>
                      <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Поиск</Typography>
                      <TextField fullWidth name="search" size="small" InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action"/></InputAdornment>) }} />
                  </Stack>
              </Grid>
              <Grid item xs={2}>
                  <Stack spacing={1} sx={{ width: '100%' }}>
                      <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Дата добавления с</Typography>
                      <TextField fullWidth name="dateFrom" size="small" type="date" InputLabelProps={{ shrink: true }} />
                  </Stack>
              </Grid>
              <Grid item xs={2}>
                  <Stack spacing={1} sx={{ width: '100%' }}>
                      <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Дата добавления по</Typography>
                      <TextField fullWidth name="dateTo" size="small" type="date" InputLabelProps={{ shrink: true }} />
                  </Stack>
              </Grid>                
              <Grid item xs={2} sm={12} md="auto" sx={{ ml: 'auto', display: 'flex', alignItems: 'flex-end' }}>
                  <Button variant="contained" color="primary" sx={{ height: 40, mt: 3.5 }} onClick={() => setIsModalOpen(true)}>
                      Добавить заказ
                  </Button>
              </Grid>                                
            </Grid>
        </Paper>

        {/* Таблица заказов */}
        <Box sx={{
          display: 'flex',
          flexDirection: 'row',
          alignItems: 'flex-start',
          /*height: '92%',*/
          width: '100%', /*27*/
        }}>                      
          <TableContainer component={Paper} sx={{ height: '100%'}}>
            <Table stickyHeader aria-label="work orders table" sx={{ minWidth: 700, height: '100%', '& .MuiTableCell-root': { verticalAlign: 'top' } }} aria-label="customized table">                   
                <TableHead>
                    <TableRow>
                        {planningWorkOrdersHeaders && planningWorkOrdersHeaders.map((col) => (
                            <StyledTableCell 
                                key={col.key} 
                                align={col.align || 'left'}
                            >
                                {col.label}
                            </StyledTableCell>
                        ))}
                    </TableRow>
                </TableHead>                  
                <TableBody sx={{ verticalAlign: 'top' }}>
                    {workOrdersItems?.map((row) => (
                    <StyledTableRow 
                        hover 
                        key={row.id}
                        selected={selectedRowId === row.id}
                        onClick={() => setSelectedRowId(prev => prev === row.id ? null : row.id)}                            
                        sx={{
                          '& .MuiTableCell-root': {
                            padding: '6px 12px',
                            height: '36px',      
                          },
                          '&.Mui-selected': {
                            backgroundColor: 'rgba(25, 118, 210, 0.25) !important',
                            '&:hover': {
                              backgroundColor: 'rgba(25, 118, 210, 0.35) !important',
                            },
                          },
                          '&:hover': {
                            backgroundColor: 'rgba(25, 118, 210, 0.15) !important',
                            cursor: 'pointer',
                            },
                        }}
                    >
                        {planningWorkOrdersHeaders.map((col) => {
                        const value = row[col.key];
                        let cellContent = null;

                        if (value == null || value === '') {
                          if (col.key == 'actions') {
                            // кнопка "Действия" не привязана к запросу
                            cellContent = (
                              <>
                                <Box sx={{ display: 'flex', flexDirection: 'row', gap: 0.5 }}>
                                  <Button 
                                    variant="outlined" 
                                    color="success" 
                                    startIcon={<AnalyticsIcon />}
                                    sx={{ minWidth: '100px' }}                                     
                                    onClick={() => showOrderDetails(selectedRowId)}
                                  >
                                    Рассчитать
                                  </Button>                                 
                                  <IconButton                                        
                                    color="primary" 
                                    onClick={(e) => { e.stopPropagation(); handleEditClick(row); }}
                                  >
                                    <EditIcon />
                                  </IconButton>
                                  <IconButton 
                                    variant="text" 
                                    color="error" 
                                    
                                  
                                    onClick={(e) => { e.stopPropagation(); handleDeleteClick(row); }}
                                  >
                                    <DeleteIcon />
                                  </IconButton>                                
                                </Box>                             
                              </>
                              );
                          } else {
                            cellContent = '—';  
                          }                               
                        } else {
                            switch (col.type) {
                            case 'date':
                                cellContent = formatDate(value); // 👈 Функция ниже
                                break;
                            case 'number':
                                // Форматируем числа с разделителями тысяч
                                cellContent = new Intl.NumberFormat('ru-RU').format(value);
                                // Если это количество — добавляем "шт."
                                if (col.key === 'quantity') cellContent += ' шт.';
                                break;
                            case 'status':
                                const config = statusConfig[value] || { label: value || '—', color: 'default' };  
                                cellContent = (
                                  <Chip
                                    label={config.label}          // 👈 Переведённое название
                                    color={config.color}          // 👈 Строка цвета (primary, success и т.д.)
                                    size="small"
                                    variant="filled"
                                    sx={{ fontWeight: 600, fontSize: '0.75rem' }}
                                  />
                                );
                                break;
                            default:
                                cellContent = value;
                                break;                                                               
                            }
                        }
                        return (
                            <StyledTableCell key={`${row.id}-${col.key}`} align={col.align || 'left'} sx={{ minWidth: '80px' }}>
                                {cellContent}
                            </StyledTableCell>
                        );
                        })}
                    </StyledTableRow>
                    ))}
                </TableBody>
            </Table>
            <TablePagination
              component="div"
              count={workOrdersTotal || 0}
              page={page}
              onPageChange={(_, newPage) => setPage(newPage)}
              rowsPerPage={pageSize}
              onRowsPerPageChange={(e) => {
                setPageSize(parseInt(e.target.value, 10));
                setPage(0);
              }}
              rowsPerPageOptions={[10, 25, 50, 100]}
              labelRowsPerPage="Записей на странице:"
              labelDisplayedRows={({ from, to, count }) => `${from}–${to} из ${count}`}
              sx={{ borderTop: '1px solid', borderColor: 'divider', mt: 'auto' }}
            />
          </TableContainer>
        </Box>
      </Box>

      {/* Модальное окно */}
      <CreateOrderModal 
        open={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          //setEditModalOpen(false);
          setEditModalData(null);
        }}
        onSave={handleSaveOrder}
        autocompleteItems={productsListItems}
        loading={workOrdersLoading || setOrderLoading}
        mode={editModalData ? 'edit' : 'create'}
        initialData={editModalData}
      />

      <OrderDetailsModal
        open={detailsModalOpen}
        onClose={() => {
          setDetailsModalOpen(false);
          setSelectedOrder(null);
        }}
        orderId={selectedOrder?.id}
        orderData={selectedOrder}
      />

      <Backdrop
        sx={{ 
          color: '#fff', 
          zIndex: (theme) => theme.zIndex.drawer + 10,
          /*transition: 'opacity 300ms'*/
        }}
        open={isSaving}
      >
        <CircularProgress color="inherit" />
      </Backdrop>
    </>
  );
}

export {Planning};