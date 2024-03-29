import React, {useEffect, useState} from 'react';
import TransactionRow from './TransactionRow';
import {useAppSelector} from '../hooks/hooks';
import {selectLoggedIn} from '../auth/authSlice';
import {useGetTransactionPageQuery} from '../api/transactionApi';
import {useGetAllUsersQuery} from '../api/usersApi';
import {useGetAllCategoriesQuery} from '../api/categoryApi';
import {Paginator} from 'primereact/paginator';

export default function List() {
  const loggedIn = useAppSelector(selectLoggedIn);
  
  const [currentPage, setCurrentPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);

  const { data: userData, error: userError } = useGetAllUsersQuery();
  const {
    data: transactionData,
    error: transactionError,
  } = useGetTransactionPageQuery({ currentPage, pageSize });
  const { data: categoryData, error: categoryError } = useGetAllCategoriesQuery();
  
  useEffect(() => {}, [currentPage, pageSize]);

  if (!loggedIn) {
    return <div>Not signed in</div>;
  }
  if (transactionError || userError || categoryError) return <div>Failed to load</div>;
  if (!transactionData || !userData) return <div>loading...</div>;
  
  const handlePageClick = (page: number) => {
    setCurrentPage(page);
  };

  function RenderedList({ currentItems }) {
    return currentItems.map((row) => {
      if (userData) {
        return (
          <TransactionRow
            key={row.id}
            row={row}
            users={userData}
            categories={categoryData}
          />
        );
      }
    });
  }

  return (
    <>
      <table id="table" className="w-full table-auto">
        <thead>
          <tr className="bg-gray-100">
            <th className="py-3">Date</th>
            <th className="py-3">Description</th>
            <th className="py-3">Category</th>
            <th className="py-3 text-right">Amount</th>
            <th className="py-3"></th>
          </tr>
        </thead>
        <tbody className='text-sm font-light'>
          <RenderedList currentItems={transactionData.payload} />
        </tbody>
      </table>
      <Paginator totalRecords={transactionData.totalNoOfItems}
                 rows={pageSize}
                 first={currentPage * pageSize}
                 onPageChange={(e) => handlePageClick(e.page)} />
    </>
  );
}
