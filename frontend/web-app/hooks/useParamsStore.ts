import { create } from "zustand";

type State = {
    pageNumber: number;
    pageSize: number;
    pageCount: number;
    searchTerm: string;
    orderBy: string;
    filterBy: string;
    seller?: string;
    winner?: string;
}

type Actions = {
    setParams: (params: Partial<State>) => void;
    reset: () => void;
}

const initialState: State = {
    pageNumber: 1,
    pageSize: 12,
    pageCount: 1,
    searchTerm: '',
    orderBy: 'make',
    filterBy: 'live',
    seller: undefined,
    winner: undefined
}

export const useParamsStore = create<State & Actions>()((set) => ({
    ...initialState,
    setParams: (newParams: Partial<State>) => {
        set((state) => {
            if (newParams.pageNumber) {
                return {...state, pageNumber: newParams.pageNumber}
            } else {
                return {...state, ...newParams, pageNumber: 1}
            }
        })
    },
    reset: () => set(initialState)
}))
