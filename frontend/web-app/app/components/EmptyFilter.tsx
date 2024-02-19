'use client'

import { useParamsStore } from '@/hooks/useParamsStore';
import React from 'react'
import Heading from './Heading';
import { Button } from 'flowbite-react';
import { signIn } from 'next-auth/react';

type Props = {
    title?: string;
    subtitle?: string;
    showReset?: boolean;
    showLogin?: boolean;
    callbackUrl?: string;
}

export default function EmptyFilter({
    title = 'No matching results',
    subtitle = 'Try adjusting your filters to find what you are looking for.',
    showReset = false,
    showLogin,
    callbackUrl
}: Props) {
    const reset = useParamsStore(state => state.reset);

    return (
    <div className='h-[40vh] flex flex-col gap-2 justify-center items-center shadow-md'>
        <Heading title={title} subtitle={subtitle} center />
        <div className='mt-4'>
            {showReset && (
                <Button outline onClick={reset}>Remove Filters</Button>
            )}
            {showLogin && (
                <Button outline onClick={() => signIn("id-server", {callbackUrl})}>Login</Button>
            )}
        </div>
    </div>
    )
}
