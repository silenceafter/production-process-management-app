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
  Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, 
  TextField,
  Typography,
  Stack,
  rgbToHex
} from '@mui/material';
import { styled } from '@mui/material/styles';
import LoadingButton from '@mui/lab/LoadingButton';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import SearchIcon from '@mui/icons-material/Search';
import { useSnackbar } from 'notistack';
//import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import CreateOrderModal from './components/CreateOrderModal';
import { 
  getOrders as workOrdersFetchData
} from '../../../store/slices/planning/workOrdersSlice';

function Planning({ showLoading }) {
  const dispatch = useDispatch();
  const StyledTableCell = styled(TableCell)(({ theme }) => ({
    '&.MuiTableCell-head': { // селектор для заголовочной клетки
      backgroundColor: 'rgb(8, 22, 39)',
      color: theme.palette.common.white,
    },
    '&.MuiTableCell-body': { // селектор для основной клетки
      fontSize: 14,
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
  const planningWorkOrdersHeaders = {
    row_num: '#',
    order_number: 'Номер заказа', 
    drawing_number: 'Наименование изделия',
    product_name: 'Номер изделия',
    quantity: 'Количество',
    planned_start: 'План.начало',
    due_date: 'Выполнить до',
    status: 'Статус', 
    created_at: 'Дата добавления'
  };
  const statusConfig = {
    'Черновик': 'primary',
    'Активен': 'info',
    'Завершён': 'success',
    'Отменён': 'error',
    'Ожидание': 'warning',    
  };
  const cards = [
    {
      title: 'Создано техпроцессов',
      value: 'Нет данных',
      link: 'Подробнее',
    },
    {
      title: 'Дата создания последнего техпроцесса',
      value: 'Нет данных',
      link: 'Подробнее',
    },
    {
      title: 'Карточка 3',
      value: 'Текст 1',
      link: 'Подробнее',
    },
    {
      title: 'Карточка 4',
      value: 'Текст 2',
      link: 'Подробнее',
    },
    {
      title: 'Карточка 5',
      value: 'Текст 3',
      link: 'Подробнее',
    },
  ];

  //стейты
    const [isSaving, setIsSaving] = useState(false);
    const [isModalOpen, setIsModalOpen] = useState(false);

  //селекторы
    const workOrdersItems = useSelector((state) => state.workOrders.items);
    const workOrdersLoading = useSelector((state) => state.workOrders.loading);

  //рефы
  //хуки
  const { enqueueSnackbar } = useSnackbar();

  //события
  const handleCreateOrder = async (newOrderData) => {
    console.log("Отправка данных:", newOrderData);
    // await dispatch(createOrder(newOrderData)).unwrap();
  };

  //эффекты
  useEffect(() => {
    dispatch(workOrdersFetchData({}));
}, []);

  // вывод
  return (
    <>      
      <Box sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
        height: '100%',
        width: '100%', /*27*/
        gap: 2
      }}>
        <Box sx={{
            display: 'flex',
            flexDirection: 'row',
            alignItems: 'flex-start',
            height: '20%',
            width: '100%', /*27*/
        }}>
            <Grid container spacing={3}>
        {cards.map((card, index) => (
          <Grid item xs={12} sm={6} md={2.4} key={index}>
            <Card 
              sx={{ 
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'space-between',
                boxShadow: '0 1px 3px rgba(0,0,0,0.12)',
                borderRadius: 2,
                '&:hover': {
                  boxShadow: '0 4px 8px rgba(0,0,0,0.15)',
                }
              }}
            >
              <CardContent sx={{ p: 2 }}>
                <Typography 
                  variant="caption" 
                  color="text.secondary"
                  sx={{ 
                    display: 'block',
                    mb: 1,
                    fontSize: '0.875rem',
                    lineHeight: 1.3
                  }}
                >
                  {card.title}
                </Typography>
                
                <Typography 
                  variant="h6" 
                  component="div"
                  sx={{ 
                    mb: 1,
                    fontSize: '1.5rem',
                    fontWeight: 400,
                    color: 'text.primary'
                  }}
                >
                  {card.value}
                </Typography>
                
                <Link 
                  href="#" 
                  variant="body2"
                  sx={{
                    fontSize: '0.875rem',
                    textDecoration: 'none',
                    '&:hover': {
                      textDecoration: 'underline',
                    }
                  }}
                >
                  {card.link}
                </Link>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
        </Box>

        {/* Фильтры и кнопки */}
        <Paper sx={{
            display: 'flex',
            flexDirection: 'row',
            alignItems: 'stretch',
            height: '20%',
            width: '100%',
            padding: 2,
        }}>
            <Grid container spacing={2} alignItems="flex-end">        
                <Grid item xs={2.4}>
                    <Stack spacing={1} sx={{ width: '100%' }}>
                        <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Поиск</Typography>
                        <TextField fullWidth name="search" size="small" InputProps={{ startAdornment: (<InputAdornment position="start"><SearchIcon color="action"/></InputAdornment>) }} />
                    </Stack>
                </Grid>
                <Grid item xs={2.4}>
                    <Stack spacing={1} sx={{ width: '100%' }}>
                        <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Дата добавления с</Typography>
                        <TextField fullWidth name="dateFrom" size="small" type="date" InputLabelProps={{ shrink: true }} />
                    </Stack>
                </Grid>
                <Grid item xs={2.4}>
                    <Stack spacing={1} sx={{ width: '100%' }}>
                        <Typography variant="subtitle2" color="text.secondary" fontWeight={500}>Дата добавления по</Typography>
                        <TextField fullWidth name="dateTo" size="small" type="date" InputLabelProps={{ shrink: true }} />
                    </Stack>
                </Grid>
                <Grid item xs={12} sm={12} md="auto" sx={{ display: 'flex', justifyContent: 'flex-start' }}>
                    <Button variant="contained" color="primary" sx={{ height: 40, mt: 3.5 }} onClick={() => setIsModalOpen(true)}>
                        Сохранить
                    </Button>
                </Grid>
            </Grid>
        </Paper>
        {/* Таблица заказов */}
        <Box sx={{
            display: 'flex',
            flexDirection: 'row',
            alignItems: 'flex-start',
            height: '60%',
            width: '100%', /*27*/
        }}>                      
            <TableContainer component={Paper} sx={{ height: '100%'}}>
                <Table sx={{ minWidth: 700, height: '100%', '& .MuiTableCell-root': { verticalAlign: 'top' } }} aria-label="customized table">
                    <TableHead>
                        <TableRow>
                            {planningWorkOrdersHeaders && Object.entries(planningWorkOrdersHeaders).map(([key, value], index) => (
                            <StyledTableCell key={key}>{value}</StyledTableCell>
                            ))}
                        </TableRow>
                    </TableHead>
                    <TableBody sx={{ verticalAlign: 'top' }}>
                        {workOrdersItems?.map((row) => (
                            <StyledTableRow key={row.id}>
                                {Object.keys(planningWorkOrdersHeaders).map((key) => (
                                    <StyledTableCell key={`${row.id}-${key}`}>
                                        {row[key] != null ? (
                                            key === 'status' ? (
                                                <Chip
                                                    label={row[key]}
                                                    color={statusConfig[row[key]] || 'default'}
                                                    size="small"
                                                    variant="filled"
                                                    sx={{ fontWeight: 600, fontSize: '0.75rem' }}
                                                />
                                                ) : row[key]            
                                            ) : ('—')
                                        }
                                    </StyledTableCell>
                                ))}
                            </StyledTableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>          
        </Box>
      </Box>

      {/* Модальное окно */}
      <CreateOrderModal 
        open={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        onSave={handleCreateOrder} 
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