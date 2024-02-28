'use client'
     
import {ReactNode, useEffect, useState} from 'react';
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {useAuctionStore} from "@/hooks/useAuctionStore";
import {Bid} from "@/types";
 
type Props = {
    children: ReactNode;
}
 
export default function SignalRProvider({children}: Props) {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
    
    useEffect(() =>{
        const newConnection = new HubConnectionBuilder()
            .withUrl('http://localhost:6001/notifications')
            .withAutomaticReconnect()
            .build();
        
        setConnection(newConnection);
    }, [])
    
    useEffect(() => {
        if (connection){
            connection.start()
                .then(() => {
                    console.log('Connected to notification hub');
                    
                    connection.on('BidPlaced', (bid:Bid) =>{
                        console.log('Bid placed event received');
                        if (bid.bidStatus.includes('Accepted')){
                            setCurrentPrice(bid.auctionId, bid.amount);
                        }
                    })
                })
                .catch(error=>console.log(error))
        }
        
        return () => {
            connection?.stop();
        }
    }, [connection, setCurrentPrice])
    
    return (
        children
    )
}
