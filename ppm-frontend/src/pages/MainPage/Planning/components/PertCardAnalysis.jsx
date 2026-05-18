import React from 'react';
import {
    Button,
  Card,
  CardHeader,
  CardContent,
  CardActions,
  Typography,
  Box,
  Chip,
  Divider,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TableContainer,
  Paper,
  IconButton,
  Tooltip, Radio, RadioGroup, FormControlLabel, FormControl, FormLabel
} from '@mui/material';
import {
  AccessTime,
  Warning,
  Timeline,
  Refresh,
  Download,
} from '@mui/icons-material';
import { styled } from '@mui/material/styles';
import dayjs from 'dayjs';
import 'dayjs/locale/ru';
dayjs.locale('ru');

// Стилизованные ячейки (как в Planning)
const StyledTableCell = styled(TableCell)(({ theme }) => ({
  '&.MuiTableCell-head': {
    backgroundColor: 'rgb(8, 22, 39)',
    color: theme.palette.common.white,
    fontWeight: 600,
    fontSize: '0.875rem',
  },
  '&.MuiTableCell-body': {
    fontSize: '0.875rem',
    py: 1,
  },
}));

const StyledTableRow = styled(TableRow)(({ theme }) => ({
  '&:nth-of-type(odd)': {
    backgroundColor: theme.palette.action.hover,
  },
}));

// Вспомогательная карточка метрики
function MetricChip({ icon, label, value, color, tooltip }) {
  return (
    <Tooltip title={tooltip}>
      <Box sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        gap: 0.5, 
        px: 1.5, 
        py: 0.75,
        bgcolor: `${color}10`,
        borderRadius: 1,
        border: `1px solid ${color}30`,
      }}>
        {icon}
        <Box sx={{ display: 'flex', flexDirection: 'column', lineHeight: 1.2 }}>
          <Typography variant="caption" color="text.secondary" fontSize="0.7rem">
            {label}
          </Typography>
          <Typography variant="body2" fontWeight={600} color={color} fontSize="0.95rem">
            {value}
          </Typography>
        </Box>
      </Box>
    </Tooltip>
  );
}

// Основной компонент
export default function PertAnalysisCard({ pertData, onRecalculate, onExport }) {
  // Форматирование времени: минуты → "7ч 9мин" или "39мин"
  const formatDuration = (minutes) => {
    if (minutes === null || minutes === undefined) return '—';
    const mins = Math.round(minutes);
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    return h > 0 ? `${h}ч ${m}мин` : `${m}мин`;
  };

  // Конфигурация сценариев
  const scenarioConfig = {
    optimistic: { label: 'Оптимистичный', color: '#2e7d32', icon: <AccessTime fontSize="small" /> },
    expected: { label: 'Ожидаемый (PERT)', color: '#1976d2', icon: <Timeline fontSize="small" /> },
    pessimistic: { label: 'Пессимистичный', color: '#d32f2f', icon: <Warning fontSize="small" /> },
  };

  const scenario = scenarioConfig[pertData?.scenario] || scenarioConfig.expected;

  // Расчёт общего резерва
  const totalSlack = pertData?.operations?.reduce((sum, op) => sum + (op.slack || 0), 0) || 0;

  return (
    <Card sx={{ 
      borderRadius: 2, 
      boxShadow: '0 2px 8px rgba(0,0,0,0.08)',
      border: '1px solid',
      borderColor: 'divider',
    }}>
      <CardHeader
        title={
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
            <Typography variant="h6" fontWeight={600}>
              Анализ сроков изготовления (PERT-анализ)
            </Typography>
            <Chip
              label={scenario.label}
              color={pertData?.scenario === 'pessimistic' ? 'error' : pertData?.scenario === 'optimistic' ? 'success' : 'primary'}
              size="small"              
              sx={{ fontWeight: 500, marginBottom: 1 }}                             
            />
          </Box>
        }
        subheader={
          <Typography variant="body2" color="text.secondary">
            Расчет: {dayjs().format('DD.MM.YYYY HH:mm')}
          </Typography>
        }
        action={
          <Box sx={{ display: 'flex', gap: 0.5 }}>
            {onRecalculate && (
              <Tooltip title="Пересчитать">
                <IconButton size="small" onClick={onRecalculate}>
                  <Refresh fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
            {onExport && (
              <Tooltip title="Экспорт">
                <IconButton size="small" onClick={onExport}>
                  <Download fontSize="small" />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        }
        sx={{ pb: 1 }}
      />

      <Divider />

      <CardContent sx={{ pt: 2, pb: 1 }}>
        <Box sx={{ 
          display: 'flex', 
          flexWrap: 'wrap', 
          gap: 1.5, 
          mb: 2,
          justifyContent: { xs: 'center', sm: 'flex-start' }
        }}>
          <MetricChip
            icon={<AccessTime sx={{ color: scenario.color }} fontSize="small" />}
            label="Длительность"
            value={formatDuration(pertData?.totalDuration)}
            color={scenario.color}
            tooltip="Общая длительность по сценарию"
          />
          
          <MetricChip
            icon={<Warning sx={{ color: '#ed6c02' }} fontSize="small" />}
            label="Критический путь"
            value={`${pertData?.criticalCount} оп.`}
            color="#ed6c02"
            tooltip="Операции без резерва времени"
          />
          
          <MetricChip
            icon={<Timeline sx={{ color: '#9c27b0' }} fontSize="small" />}
            label="Общий резерв"
            value={formatDuration(totalSlack)}
            color="#9c27b0"
            tooltip="Суммарный резерв всех операций"
          />
        </Box>

        <Divider sx={{ my: 1.5 }} />
        <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1, color: 'text.primary' }}>
          Операции заказа
        </Typography>
        
        <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1, maxHeight: 400 }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                <StyledTableCell width="15%">Код</StyledTableCell>
                <StyledTableCell align="right" width="20%">Длит.</StyledTableCell>
                <StyledTableCell align="right" width="20%">Старт</StyledTableCell>
                <StyledTableCell align="right" width="20%">Финиш</StyledTableCell>
                <StyledTableCell align="right" width="15%">Резерв</StyledTableCell>
                <StyledTableCell align="center" width="10%">Путь</StyledTableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {pertData?.operations?.map((op) => {
                const finish = op.earliestStart + op.durationExpected;
                return (
                  <StyledTableRow 
                    key={op.id}
                    sx={{ 
                      bgcolor: op.isCriticalPath ? 'rgba(211, 47, 47, 0.04)' : 'inherit',
                      borderLeft: op.isCriticalPath ? '3px solid #d32f2f' : '3px solid transparent',
                      '&:hover': { bgcolor: op.isCriticalPath ? 'rgba(211, 47, 47, 0.08)' : 'action.hover' }
                    }}
                  >
                    <TableCell component="th" scope="row" fontWeight={op.isCriticalPath ? 600 : 400}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        {op.operationCode}
                        {op.isCriticalPath && (
                          <Tooltip title="Критическая операция">
                            <Warning color="error" fontSize="inherit" sx={{ opacity: 0.7 }} />
                          </Tooltip>
                        )}
                      </Box>
                    </TableCell>
                    <TableCell align="right" fontWeight={500}>
                      {formatDuration(op.durationExpected)}
                    </TableCell>
                    <TableCell align="right" color="text.secondary">
                      {formatDuration(op.earliestStart)}
                    </TableCell>
                    <TableCell align="right" color="text.secondary">
                      {formatDuration(finish)}
                    </TableCell>
                    <TableCell align="right" sx={{ 
                      color: op.slack > 0 ? '#2e7d32' : '#d32f2f',
                      fontWeight: op.slack === 0 ? 600 : 400,
                      fontFamily: 'monospace',
                      fontSize: '0.8rem'
                    }}>
                      {op.slack > 0 ? formatDuration(op.slack) : '—'}
                    </TableCell>
                    <TableCell align="center">
                      <Chip
                        label={op.isCriticalPath ? '🔴' : '🟢'}
                        size="small"
                        variant="outlined"
                        sx={{ 
                          minWidth: 28, 
                          width: 28, 
                          height: 24,
                          '& .MuiChip-label': { px: 0.5 }
                        }}
                      />
                    </TableCell>
                  </StyledTableRow>
                );
              })}
              {(!pertData?.operations || pertData.operations.length === 0) && (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 3, color: 'text.secondary' }}>
                    Нет данных об операциях
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>

        <Box sx={{ mt: 2, p: 1.5, bgcolor: 'info.light', borderRadius: 1, display: 'flex', gap: 1, alignItems: 'start' }}>
          <Warning color="info" fontSize="small" sx={{ mt: 0.25 }} />
          <Typography variant="body2" color="info.dark" fontSize="0.8rem">
            <strong>Критический путь</strong> — задержка любой из помеченных операций сдвинет срок всего заказа. 
            Операции с резервом можно сдвигать в пределах указанного времени.
          </Typography>
        </Box>
      </CardContent>      
    </Card>
  );
}