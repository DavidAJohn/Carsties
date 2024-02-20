import React from 'react'
import { AiOutlineCar } from 'react-icons/ai'
import Search from './Search'
import LoginButton from './LoginButton'
import { getCurrentUser } from '../actions/authActions';
import UserActions from './UserActions';
import Link from 'next/link';

export default async function Navbar() {
  const user = await getCurrentUser();

  return (
    <header className='sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md'>
        <div className='text-3xl font-semibold text-red-500'>
            <Link href='/' className='flex items-center gap-2 '>
              <AiOutlineCar size={34} />
              <div>Carsties Auctions</div>
            </Link>
        </div>
        <Search />
        {user ? (
          <UserActions user={user} />
        ) : (
          <LoginButton />
        )}
    </header>
  )
}
