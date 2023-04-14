import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'
import { ConfigProvider } from 'antd';
import ruRu from 'antd/locale/ru_RU';
import {ApiClient} from "./apiClient";

const apiClient = new ApiClient("https://localhost:7025")
ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
    <React.StrictMode>
        <ConfigProvider locale={ruRu}>
            <App apiClient={apiClient}/>
        </ConfigProvider>
    </React.StrictMode>,
)
