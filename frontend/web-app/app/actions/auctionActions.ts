'use server'

import { Auction, PagedResult } from "@/types";

export async function getData(query: string): Promise<PagedResult<Auction>> {
    const response = await fetch(`http://localhost:6001/search${query}`);
  
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
  
    return response.json();
}