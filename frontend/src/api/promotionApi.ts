import axiosInstance from './axiosInstance';
import type { Promotion, CreatePromotionRequest, UpdatePromotionRequest } from '@/types/promotion.types';

export const promotionApi = {
  getAllPromotions: async (): Promise<Promotion[]> => {
    const response = await axiosInstance.get<{ data: Promotion[] }>('/promotions');
    return response.data.data;
  },

  getPromotionById: async (id: number): Promise<Promotion> => {
    const response = await axiosInstance.get<{ data: Promotion }>(`/promotions/${id}`);
    return response.data.data;
  },

  createPromotion: async (data: CreatePromotionRequest): Promise<Promotion> => {
    const response = await axiosInstance.post<{ data: Promotion }>('/promotions', data);
    return response.data.data;
  },

  updatePromotion: async (id: number, data: UpdatePromotionRequest): Promise<Promotion> => {
    const response = await axiosInstance.put<{ data: Promotion }>(`/promotions/${id}`, data);
    return response.data.data;
  },

  deletePromotion: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/promotions/${id}`);
  }
};
