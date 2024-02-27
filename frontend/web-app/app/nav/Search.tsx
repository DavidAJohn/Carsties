'use client'

import { useParamsStore } from '@/hooks/useParamsStore';
import { usePathname, useRouter } from 'next/navigation';
import React, { useState } from 'react'
import { FaSearch } from 'react-icons/fa'
import { FaArrowRotateLeft } from "react-icons/fa6";

export default function Search() {
    const router = useRouter();
    const pathname = usePathname();

    const setParams = useParamsStore(state => state.setParams);
    const reset = useParamsStore(state => state.reset);
    const [value, setValue] = useState('');

    function onChange(event: any) {
        setValue(event.target.value);
    }

    function search() {
        if (value === '') return;
        if (pathname !== '/') router.push('/');
        setParams({searchTerm: value});
    }

    function resetSearch() {
        reset();
        setParams({searchTerm: ''});
        setValue('');
    }

    return (
        <div className='flex w-1/4 items-center border-2 rounded-full py-2 shadow-sm'>
            <input 
                onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                        search();
                    }
                }}
                onChange={onChange}
                type='text' 
                className='input-custom text-sm text-gray-600' 
                placeholder='Search for cars by make, model or colour...' 
                value={value}
            />
            <button onClick={search} className='flex justify-end'>
                <FaSearch 
                    size={34} 
                    className='bg-red-400 text-white rounded-full p-2 cursor-pointer hover:bg-red-500 ml-2'
                />
            </button>
            <button onClick={resetSearch} className='flex justify-end'>
                <FaArrowRotateLeft 
                    size={34} 
                    className='bg-red-400 text-white rounded-full p-2 cursor-pointer hover:bg-red-500 ml-1 mr-2'
                />
            </button>
        </div>
    )
}
