import { useState } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Button, Stack, FormControl, InputLabel, Select, MenuItem
} from '@mui/material';
//import dayjs from 'dayjs';

export default function CreateOrderModal({ open, onClose, onSave }) {
  // Локальный стейт формы
  const [formData, setFormData] = useState({
    orderNumber: '',
    productName: '',
    productCode: '',
    quantity: '',
    /*startDate: dayjs(),
    deadline: dayjs().add(7, 'day'),*/
    status: 'Draft'
  });

  const handleChange = (field) => (e) => {
    setFormData(prev => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = () => {
    // Валидация перед отправкой
    if (!formData.orderNumber || !formData.productName) return;
    
    onSave(formData); // Отправляем данные наверх
    onClose();        // Закрываем модалку
    // Сброс формы можно сделать здесь или в useEffect при открытии
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Создать новый заказ</DialogTitle>
      
      <DialogContent dividers sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 3 }}>
        
        {/* Номер заказа */}
        <TextField
          label="Номер заказа"
          value={formData.orderNumber}
          onChange={handleChange('orderNumber')}
          size="small"
          autoFocus
        />

        {/* Наименование изделия */}
        <TextField
          label="Наименование изделия"
          value={formData.productName}
          onChange={handleChange('productName')}
          size="small"
        />

        {/* Количество и Код (в одну строку) */}
        <Stack direction="row" spacing={2}>
          <TextField
            label="Код изделия"
            value={formData.productCode}
            onChange={handleChange('productCode')}
            size="small"
            sx={{ flex: 1 }}
          />
          <TextField
            label="Количество"
            type="number"
            value={formData.quantity}
            onChange={handleChange('quantity')}
            size="small"
            sx={{ width: 100 }}
          />
        </Stack>

        {/* Даты */}
        <Stack direction="row" spacing={2}>
           {/* Здесь лучше использовать DatePicker из @mui/x-date-pickers, 
               но для простоты примера оставил TextField type="date" */}
           {/*<TextField
             label="План. начало"
             type="date"
             size="small"
             InputLabelProps={{ shrink: true }}
             value={formData.startDate.format('YYYY-MM-DD')}
             onChange={(e) => setFormData({...formData, startDate: dayjs(e.target.value)})}
             sx={{ flex: 1 }}
           />
           <TextField
             label="Выполнить до"
             type="date"
             size="small"
             InputLabelProps={{ shrink: true }}
             value={formData.deadline.format('YYYY-MM-DD')}
             onChange={(e) => setFormData({...formData, deadline: dayjs(e.target.value)})}
             sx={{ flex: 1 }}
           />*/}
        </Stack>

      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} color="inherit">Отмена</Button>
        <Button onClick={handleSubmit} variant="contained" color="primary">
          Создать
        </Button>
      </DialogActions>
    </Dialog>
  );
}