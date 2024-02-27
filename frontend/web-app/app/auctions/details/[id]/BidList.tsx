'use client'

import { getBidsForAuction } from '@/app/actions/auctionActions'
import Heading from '@/app/components/Heading'
import { useBidStore } from '@/hooks/useBidStore'
import { Auction, Bid } from '@/types'
import { User } from 'next-auth'
import React, { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import BidItem from './BidItem'

type Props = {
    user: User | null
    auction: Auction
}

export default function BidList({ user, auction }: Props) {
    const [loading, setLoading] = useState(true)

    const bids = useBidStore(state => state.bids)
    const setBids = useBidStore(state => state.setBids)

    useEffect(() => {
        getBidsForAuction(auction.id)
            .then((res: any) => {
                if (res.error) {
                    throw res.error
                }

                setBids(res as Bid[])
            }).catch(err => {
                toast.error(err.message)
            }).finally(() => {
                setLoading(false)
            })
    }, [auction.id, setBids, setLoading])

    if (loading) return <div>Loading...</div>

    return (
        <div className='border-2 rounded-lg p-2 bg-gray-100'>
          <Heading title='Bids' />
          {bids.map(bid => (
            <BidItem key={bid.id} bid={bid} />
          ))}
        </div>
    )
}
