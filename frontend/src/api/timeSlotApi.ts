import axiosInstance from './axiosInstance';

export interface TimeSlot {
  slotId: number;
  slotName: string;
  startTime: string; // HH:mm:ss
  endTime: string;   // HH:mm:ss
  dayType: string;
  isActive: boolean;
}

export const timeSlotApi = {
  getAll: async () => {
    const res = await axiosInstance.get('/timeslots');
    return res.data.data as TimeSlot[];
  }
};
