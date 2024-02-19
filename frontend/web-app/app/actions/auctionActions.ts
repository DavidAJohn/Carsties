'use server'

import { Auction, PagedResult } from "@/types";
import { getTokenWorkaround } from "./authActions";

export async function getData(query: string): Promise<PagedResult<Auction>> {
    const response = await fetch(`http://localhost:6001/search${query}`);
  
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
  
    return response.json();
}

export async function UpdatedAuctionTest() {
    const data = {
        mileage: Math.floor(Math.random() * 100000) + 1
    }

    const token = await getTokenWorkaround();

    const res = await fetch('http://localhost:6001/auctions/bbab4d5a-8565-48b1-9450-5ac2a5c4a654', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token?.access_token
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) return {status: res.status, message: res.statusText};

    return res.statusText;
}