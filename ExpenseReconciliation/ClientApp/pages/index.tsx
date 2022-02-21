import Head from 'next/head'
import { useSession } from 'next-auth/client'
import UploadFile from '../components/uploadFile'
import Card from '../components/Card'
import Layout from '../components/Layout'
import Icon from '../components/Icon'

export default function Home({}) {
  const [session, loading] = useSession()

  const header = (
    <Head>
      <title>Expenses Reconciliation</title>
      <meta name='description' content='Generated by create next app' />
      <link rel='icon' href='/favicon.ico' />
    </Head>
  )

  const signedInBody = (
    <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols:3 px-5 md:gap-3 gap-y-7'>
      <Card link='/dashboard'>
        <Icon icon='pie-chart' classes='w-20 mx-auto' />
        <p className='text-center font-semibold'>Dashboard</p>
      </Card>

      <UploadFile />

      <Card link='/list'>
        <Icon icon='clipboard' classes='w-20 mx-auto' />
        <p className='text-center font-semibold'>Reconcile transactions</p>
      </Card>

      <Card>
        <Icon icon='sort-asc' classes='w-20 mx-auto' />
        <p className='text-center font-semibold'>Categorise transactions</p>
      </Card>
    </div>
  )

  const signedOutBody = (
    <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols:3 px-5 md:gap-3 gap-y-7'>
      <Card link='/api/auth/signin'>
        <Icon icon='profile' classes='w-20 mx-auto' />
        <p className='text-center font-semibold'>Sign in</p>
      </Card>
    </div>
  )

  return (
    <Layout>
      {header}

      {session ? signedInBody : signedOutBody}
    </Layout>
  )
}
