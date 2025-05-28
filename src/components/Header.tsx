import React from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import NotificationsActiveIcon from '@mui/icons-material/NotificationsActive';

interface HeaderProps {
  isMonitoring: boolean;
  isConnecting?: boolean;
  onStartMonitoring: () => void;
  lastAlert?: {
    severity: string;
    timestamp: string;
  };  alertStats?: {
    Critica: number;
    Media: number;
    Baja: number;
  };
}

const Header: React.FC<HeaderProps> = ({ 
  isMonitoring, 
  isConnecting = false, 
  onStartMonitoring, 
  lastAlert,
  alertStats 
}) => {
  return (
    <AppBar 
      position="static" 
      sx={{ 
        bgcolor: '#1976d2',
        boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)'
      }}
    >
      <Toolbar sx={{ minHeight: '70px' }}>
        <NotificationsActiveIcon 
          sx={{ 
            mr: 2, 
            fontSize: '2rem',
            animation: isMonitoring ? 'pulse 2s infinite' : 'none',
            '@keyframes pulse': {
              '0%': { opacity: 1 },
              '50%': { opacity: 0.5 },
              '100%': { opacity: 1 }
            }
          }} 
        />
        <Typography 
          variant="h5" 
          component="div" 
          sx={{ 
            flexGrow: 1,
            fontWeight: 600
          }}
        >
          Monitor de Alertas Hospitalarias
        </Typography>        {alertStats && (
          <Box sx={{ mr: 4, display: 'flex', gap: 3 }}>
            <Box sx={{ textAlign: 'center' }}>              <Typography variant="h6" color="error.light">{alertStats.Critica}</Typography>
              <Typography variant="caption" sx={{ color: 'rgba(255, 255, 255, 0.7)' }}>Críticas</Typography>
            </Box>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h6" color="warning.light">{alertStats.Media}</Typography>
              <Typography variant="caption" sx={{ color: 'rgba(255, 255, 255, 0.7)' }}>Medias</Typography>
            </Box>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h6" color="success.light">{alertStats.Baja}</Typography>
              <Typography variant="caption" sx={{ color: 'rgba(255, 255, 255, 0.7)' }}>Bajas</Typography>
            </Box>
          </Box>
        )}
        
        {lastAlert && (
          <Box sx={{ mr: 3, display: 'flex', alignItems: 'center' }}>
            <Typography 
              variant="body2" 
              sx={{ 
                color: 'rgba(255, 255, 255, 0.9)',
                backgroundColor: 'rgba(0, 0, 0, 0.1)',
                padding: '8px 16px',
                borderRadius: '4px'
              }}
            >
              Última alerta: <strong>{lastAlert?.severity}</strong> - {lastAlert?.timestamp}
            </Typography>
          </Box>
        )}
        
        <Button
          variant="contained"
          color={isMonitoring ? "inherit" : "success"}
          disabled={isMonitoring || isConnecting}
          onClick={onStartMonitoring}
          sx={{
            minWidth: '180px',
            height: '45px',
            bgcolor: isMonitoring ? 'rgba(255, 255, 255, 0.3)' : '#2e7d32',
            '&:hover': {
              bgcolor: isMonitoring ? 'rgba(255, 255, 255, 0.2)' : '#1b5e20'
            },
            transition: 'all 0.3s ease',
            position: 'relative',
            overflow: 'hidden'
          }}
        >
          {isConnecting ? 'Conectando...' : isMonitoring ? 'Monitoreando...' : 'Iniciar Monitoreo'}
          {isConnecting && (
            <Box
              sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                height: '2px',
                width: '100%',
                bgcolor: '#fff',
                animation: 'progress 1s infinite linear',
                '@keyframes progress': {
                  '0%': { transform: 'translateX(-100%)' },
                  '100%': { transform: 'translateX(100%)' }
                }
              }}
            />
          )}
        </Button>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
