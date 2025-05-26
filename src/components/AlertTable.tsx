import React from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Box
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';

interface Alert {
  Id: string | number;
  Type: string | number;
  Severity: string | number;
  Message: string;
  Location: string;
  CreatedBy: string;
  Timestamp: string;
}

interface AlertTableProps {
  alerts: Alert[];
}

const getAlertType = (type: string | number) => {
  const types = ['Emergencia', 'Enfermeria', 'Mantenimiento'];
  if (typeof type === 'number' && type >= 0 && type < types.length) {
    return types[type];
  }
  return String(type);
};

const getAlertSeverity = (severity: string | number) => {
  const severities = ['Baja', 'Media', 'Alta', 'Critica'];
  if (typeof severity === 'number' && severity >= 0 && severity < severities.length) {
    return severities[severity];
  }
  return String(severity);
};

const getSeverityColor = (severity: string | number) => {  
  const severityStr = getAlertSeverity(severity);
  switch (severityStr.toLowerCase()) {
    case 'critica':
      return {
        background: 'rgba(255, 235, 238, 0.9)',
        color: '#b71c1c',
        fontWeight: 'bold'
      };
    case 'alta':
    case 'media':
      return {
        background: 'rgba(255, 253, 231, 0.9)',
        color: '#827717'
      };
    case 'baja':
      return {
        background: 'rgba(232, 245, 233, 0.9)',
        color: '#1b5e20'
      };
    default:
      return {};
  }
};

const StyledTableContainer = styled(TableContainer)(({ theme }) => ({
  boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
  borderRadius: theme.shape.borderRadius,
  '& .MuiTableCell-head': {
    fontWeight: 'bold',
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.primary.contrastText,
  },
  '& .MuiTableRow-root': {
    '&:hover': {
      backgroundColor: 'rgba(0, 0, 0, 0.04)',
    },
    transition: 'background-color 0.2s ease',
  },
}));

const AnimatedTableRow = styled(TableRow)({
  animation: 'fadeIn 0.5s ease-in',
  '@keyframes fadeIn': {
    '0%': {
      opacity: 0,
      transform: 'translateY(10px)',
    },
    '100%': {
      opacity: 1,
      transform: 'translateY(0)',
    },
  },
});

const formatTimestamp = (timestamp: string) => {
  try {
    console.log('Formatting timestamp:', timestamp);
    if (!timestamp) {
      return 'Sin fecha';
    }
    
    let date;
    if (typeof timestamp === 'string') {
      // Intentar como fecha ISO
      try {
        date = parseISO(timestamp);
        if (!isNaN(date.getTime())) {
          return format(date, 'dd/MM/yyyy HH:mm:ss', { locale: es });
        }
      } catch (e) {
        console.log('Error parsing ISO date:', e);
      }

      // Intentar como timestamp Unix (milliseconds)
      const timestampNum = parseInt(timestamp);
      if (!isNaN(timestampNum)) {
        date = new Date(timestampNum);
        if (!isNaN(date.getTime())) {
          return format(date, 'dd/MM/yyyy HH:mm:ss', { locale: es });
        }
      }

      // Si todo falla, devolver la fecha como está
      return timestamp;
    }
    
    return 'Formato inválido';
  } catch (error) {
    console.error('Error formatting timestamp:', error);
    return 'Error en fecha';
  }
};

const AlertTable: React.FC<AlertTableProps> = ({ alerts }) => {
  return (
    <Box sx={{ width: '100%', overflow: 'hidden' }}>
      {alerts.length === 0 ? (
        <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
          No hay alertas para mostrar
        </Typography>
      ) : (
        <Paper>
          <StyledTableContainer>
            <Table stickyHeader>
              <TableHead>
                <TableRow>
                  <TableCell>ID</TableCell>
                  <TableCell>Tipo</TableCell>
                  <TableCell>Severidad</TableCell>
                  <TableCell>Mensaje</TableCell>
                  <TableCell>Ubicación</TableCell>
                  <TableCell>Creado por</TableCell>
                  <TableCell>Fecha</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {alerts.map((alert) => (
                  <AnimatedTableRow
                    key={alert.Id}
                    sx={getSeverityColor(alert.Severity)}
                  >
                    <TableCell>{alert.Id}</TableCell>
                    <TableCell>{getAlertType(alert.Type)}</TableCell>
                    <TableCell>{getAlertSeverity(alert.Severity)}</TableCell>
                    <TableCell>{alert.Message || 'Sin mensaje'}</TableCell>
                    <TableCell>{alert.Location || 'Sin ubicación'}</TableCell>
                    <TableCell>{alert.CreatedBy || 'Sistema'}</TableCell>
                    <TableCell>{formatTimestamp(alert.Timestamp)}</TableCell>
                  </AnimatedTableRow>
                ))}
              </TableBody>
            </Table>
          </StyledTableContainer>
        </Paper>
      )}
    </Box>
  );
};

export default AlertTable;
