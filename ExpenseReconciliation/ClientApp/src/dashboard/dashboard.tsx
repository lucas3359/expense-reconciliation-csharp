import React from 'react';
import useSWR from 'swr';
import User from '../model/user';
import Total from '../model/total';

export default function Dashboard() {
  const { data: totalsData, error: totalsError } = useSWR<Total[], any>(
    '/api/dashboard/GetAmountAsync',
  );
  const { data: userData, error: userError } = useSWR<User[], any>('/api/user');

  if (totalsError || userError) return <div>Failed to load</div>;
  if (!totalsData || !userData) return <div>loading...</div>;
  console.log('TotalsData');
  console.log(totalsData);

  const getUser = (userId: number): string | undefined => {
    return userData.find((user) => user.id === userId)?.name;
  };

  const renderCurrency = (amount: number): string => {
    return (amount / 100).toFixed(2);
  };

  const getTotals = () => {
    return totalsData.map((total) => {
      return (
        <div>
          <span>
            <strong>{getUser(total.userId)} </strong>
          </span>
          <span>${renderCurrency(total.amount)}</span>
        </div>
      );
    });
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols:3 px-5 md:gap-3 gap-y-7">
      <h1 className="text-4xl text-gray-700">Dashboard</h1>
      <br />
      <h2 className="text-xl text-gray-500">Amounts owing</h2>
      <br />
      <div>{getTotals()}</div>
    </div>
  );
}
