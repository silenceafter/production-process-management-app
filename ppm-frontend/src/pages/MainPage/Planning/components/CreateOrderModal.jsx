import { useState, useEffect } from 'react';
import {
  Autocomplete, Dialog, DialogTitle, DialogContent, DialogActions, ListItemText,
  TextField, Button, Stack, CircularProgress, Alert
} from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers';
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
dayjs.extend(utc); 

export default function CreateOrderModal({ 
  open, 
  onClose, 
  onSave, 
  autocompleteItems, 
  loading,
  mode = 'create',           
  initialData = null        
}) {
  const [formData, setFormData] = useState({
    orderNumber: '',
    quantity: 1,
    plannedStart: dayjs(),
    dueDate: dayjs().add(7, 'day'),
    product_id: null,
  });
  
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    if (open) {
      if (mode === 'edit' && initialData) {
        // Режим редактирования (предзаполняем поля)
        setFormData({
          orderNumber: initialData.order_number || '',
          quantity: initialData.quantity || 1,
          plannedStart: initialData.planned_start ? dayjs(initialData.planned_start) : dayjs(),
          dueDate: initialData.due_date ? dayjs(initialData.due_date) : dayjs().add(7, 'day'),
        });

        // Если есть продукт — выбираем его
        if (initialData.product_id && autocompleteItems?.length) {
          const product = autocompleteItems.find(p => p.id === initialData.product_id);
          if (product) setSelectedProduct(product);
        }
      } else {
        // Режим создания
        setFormData({
          orderNumber: '',
          quantity: 1,
          plannedStart: dayjs(),
          dueDate: dayjs().add(7, 'day'),
        });
        setSelectedProduct(null);
      }
      setError('');
    }
  }, [open, mode, initialData, autocompleteItems]);

  const handleReset = () => {
    setFormData({
      orderNumber: '',
      quantity: 1,
      plannedStart: dayjs(),
      dueDate: dayjs().add(7, 'day'),
    });
    setSelectedProduct(null);
    setError('');
    onClose();
  };

  const handleSubmit = async () => {
    setError('');

    // Валидация
    if (!selectedProduct) {
      setError('Выберите изделие');
      return;
    }
    if (formData.dueDate && formData.plannedStart && !formData.dueDate.isAfter(formData.plannedStart)) {
      setError('Срок должен быть позже даты начала');
      return;
    }

    const payload = {
      ...formData,
      productId: selectedProduct?.id,
      productName: selectedProduct?.name,
      productCode: selectedProduct?.code,
      productNumber: selectedProduct?.number,
      plannedStart: formData.plannedStart ? dayjs(formData.plannedStart).utc().format() : null,
      dueDate:  formData.dueDate  ? dayjs(formData.dueDate).utc().format()  : null,
    };

    if (mode === 'edit' && initialData?.id) {
      payload.id = initialData.id;
    }

    await onSave(payload);
    // Не закрываем модалку сразу — родитель сделает это после успешного ответа
  };

  return (
    <Dialog open={open} onClose={handleReset} maxWidth="sm" fullWidth>
      <DialogTitle>
        {mode === 'edit' ? 'Редактировать заказ' : 'Создать новый заказ'}
      </DialogTitle>      
      <DialogContent dividers sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>        
        {error && <Alert severity="error" sx={{ mb: 1 }}>{error}</Alert>}
        {/* Изделие (Autocomplete) */}
        <Autocomplete
          options={autocompleteItems}
          getOptionLabel={(opt) => `${opt.code} — ${opt.name} №${opt.number}`}
          filterOptions={(options, { inputValue }) => {
            const search = inputValue.toLowerCase();
            return options.filter(opt =>
              opt.code?.toLowerCase().includes(search) ||
              opt.name?.toLowerCase().includes(search)
            );
          }}
          renderInput={(params) => (
            <TextField {...params} label="Изделие" size="small" fullWidth required />
          )}
          renderOption={(props, option) => (
            <li {...props} key={option.id}>
              <ListItemText
                primary={option.code}
                secondary={`${option.name} №${option.number}`}
                primaryTypographyProps={{ fontWeight: 600 }}
              />
            </li>
          )}
          onChange={(e, value) => {
            setSelectedProduct(value);
            setError('');
          }}
          value={selectedProduct}
          isOptionEqualToValue={(opt, val) => opt?.id === val?.id}
        />

        {/* Количество */}
        <TextField
          label="Количество"
          type="number"
          value={formData.quantity}
          onChange={(e) => setFormData(prev => ({ ...prev, quantity: parseInt(e.target.value) || 1 }))}
          size="small"
          inputProps={{ min: 1 }}
        />

        {/* Даты */}
        <Stack direction="row" spacing={2}>
          <DateTimePicker
            label="Приступить"
            value={formData.plannedStart}
            onChange={(val) => setFormData(prev => ({ ...prev, plannedStart: val || dayjs() }))}
            format="DD.MM.YYYY HH:mm"
            minDate={dayjs()}
            slotProps={{ textField: { size: 'small', fullWidth: true } }}
          />
          <DateTimePicker
            label="Выполнить"
            value={formData.dueDate}
            onChange={(val) => setFormData(prev => ({ ...prev, dueDate: val || dayjs() }))}
            format="DD.MM.YYYY HH:mm"
            minDate={formData.plannedStart}
            slotProps={{ textField: { size: 'small', fullWidth: true } }}
          />
        </Stack>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={handleReset} disabled={loading}>Отмена</Button>
        <Button 
          onClick={handleSubmit} 
          variant="contained" 
          disabled={loading}
          startIcon={loading && <CircularProgress size={16} color="inherit" />}
        >
          {mode === 'edit' ? 'Сохранить изменения' : 'Создать'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}