'use server'

import { Auction, PagedResult } from "@/types";

export async function getData(pageNumber: number, pageSize: number): Promise<PagedResult<Auction>> {
    const response = await fetch(`http://localhost:6001/search?pageSize=${pageSize}&pageNumber=${pageNumber}`);
  
    if (!response.ok) {
      throw new Error('Network response was not ok');
    }
  
    return response.json();
}