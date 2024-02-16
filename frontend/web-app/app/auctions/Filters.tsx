import { useParamsStore } from '@/hooks/useParamsStore';
import { Button } from 'flowbite-react';
import ButtonGroup from 'flowbite-react/lib/esm/components/Button/ButtonGroup';
import React from 'react'
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill } from 'react-icons/bs';

const pageSizeButtons = [4, 8, 12];
const orderByButtons = [
    {
        label: 'Alphabetical (By Make)',
        icon: AiOutlineSortAscending,
        value: 'make'
    },
    // {
    //     label: 'Alphabetical (By Model)',
    //     icon: AiOutlineSortAscending,
    //     value: 'model'
    // },
    {
        label: 'End Date',
        icon: AiOutlineClockCircle,
        value: 'endingSoon'
    },
    {
        label: 'Recently Added',
        icon: BsFillStopCircleFill,
        value: 'new'
    }
]

export default function Filters() {
  const pageSize = useParamsStore(state => state.pageSize);
  const setParams = useParamsStore(state => state.setParams);
  const orderBy = useParamsStore(state => state.orderBy);

  return (
    <div className='flex justify-between items-center mb-4'>
        <div>
            <span className='uppercase text-sm text-gray-500 mr-2'>Order By</span>
            <ButtonGroup>
                {orderByButtons.map(({label, icon: Icon, value}) => (
                    <Button 
                        key={value}
                        onClick={() => setParams({orderBy: value})}
                        color={`${orderBy === value ? 'red' : 'gray'}`}
                        className='focus:ring-0'
                    >
                        <Icon className='mr-3 h-4 w-4' />
                        {label}
                    </Button>
                ))}
            </ButtonGroup>
        </div>
        <div>
            <span className='uppercase text-sm text-gray-500 mr-2'>Page Size</span>
            <ButtonGroup>
                {pageSizeButtons.map((value, i) => (
                    <Button
                        key={i}
                        onClick={() => setParams({pageSize: value})}
                        color={`${pageSize === value ? 'red' : 'gray'}`}
                        className='focus:ring-0'
                    >
                        {value}
                    </Button>
                ))}
            </ButtonGroup>
        </div>
    </div>
  )
}
