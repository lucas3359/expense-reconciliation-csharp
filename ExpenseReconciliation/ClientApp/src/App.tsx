import React from 'react';
import './App.css';
import {BrowserRouter, Route, Routes} from 'react-router-dom';
import Home from './home/home';
import Dashboard from './dashboard/dashboard';
import List from './transactions/list';
import Header from './components/Header';
import Footer from './components/Footer';
import Categories from './categories/categories';

function App() {   
  return (
    <div
      id="app-root"
      className="bg-gradient-to-tl bg-gradient-to-r from-indigo-100 via-red-100 to-yellow-100"
      data-theme='pastel'
    >
      <div className="flex flex-col min-h-screen backdrop-filter backdrop-saturate-50">
        <BrowserRouter>
          <Header />
          <main className="container mx-auto py-5 px-2 flex-grow">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/dashboard" element={<Dashboard />} />
              <Route path="/list" element={<List />} />
              <Route path="/categories" element={<Categories />} />
            </Routes>
          </main>
          <Footer />
        </BrowserRouter>
      </div>
    </div>
  );
}

export default App;
