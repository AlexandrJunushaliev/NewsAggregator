import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'
import {ruRU} from '@mui/material/locale';
import {createTheme, ThemeProvider} from '@mui/material/styles';
import {ApiClient} from "./apiClient";

const theme = createTheme(
    ruRU
);
const apiClient = new ApiClient("https://localhost:7025")
ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
    <React.StrictMode>
        <ThemeProvider theme={theme}>
            <App apiClient={apiClient}/>
        </ThemeProvider>
    </React.StrictMode>,
)
