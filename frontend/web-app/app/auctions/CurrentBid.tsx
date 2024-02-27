import React from 'react'

type Props = {
    amount?: number
    reservePrice: number
}

export default function CurrentBid({amount, reservePrice}: Props) {
    const text = amount ? '$' + amount : 'No bids yet'
    const color = amount ? amount > reservePrice ? 'bg-green-600' : 'bg-amber-600' : 'bg-red-600'

    return (
    <div className={`
        py-1
        px-2
        ${color}
        text-white
        border-2
        border-white
        rounded-lg
        flex
        justify-center
    `}>
        {text}
    </div>
    )
}
