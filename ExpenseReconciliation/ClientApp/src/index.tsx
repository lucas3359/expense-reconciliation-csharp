import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import 'primereact/resources/themes/tailwind-light/theme.css';     //theme
import 'primereact/resources/primereact.min.css';                  //core css
import 'primeicons/primeicons.css';                                //icons
import App from './App';
import reportWebVitals from './reportWebVitals';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { store } from './store';
import { Provider } from 'react-redux';

const clientId = process.env.REACT_APP_GOOGLE_CLIENT_ID
  || document.querySelector('meta[property="clientId"]')?.getAttribute('content')
  || 'clientIdNotSet';

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement,
);
root.render(
  <React.StrictMode>
    <Provider store={store}>
      <GoogleOAuthProvider clientId={clientId}>
        <App />
      </GoogleOAuthProvider>
    </Provider>
  </React.StrictMode>,
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
