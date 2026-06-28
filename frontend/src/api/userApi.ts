import axiosInstance from './axiosInstance';

export interface UserSummary {
  userId: number;
  fullName: string;
  email: string;
  phone: string;
  avatarUrl?: string;
  role: string;
  isActive: boolean;
}

export const userApi = {
  getAll: async () => {
    const res = await axiosInstance.get('/users');
    return res.data.data as UserSummary[];
  }
};
