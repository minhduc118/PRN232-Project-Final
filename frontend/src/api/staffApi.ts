import axiosInstance from './axiosInstance';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

// Helper to load/save mock equipment in LocalStorage
const getMockEquipment = (): any[] => {
  const local = localStorage.getItem('mock_equipment_inventory');
  if (local) return JSON.parse(local);

  const defaults = [
    { inventoryId: 1, serviceId: 1, serviceName: 'Thuê vợt cầu lông', itemCode: 'EQ-001', condition: 'Good', purchaseDate: '2026-03-01T00:00:00Z', purchasePrice: 450000, isAvailable: true },
    { inventoryId: 2, serviceId: 1, serviceName: 'Thuê vợt cầu lông', itemCode: 'EQ-002', condition: 'Good', purchaseDate: '2026-03-01T00:00:00Z', purchasePrice: 450000, isAvailable: true },
    { inventoryId: 3, serviceId: 1, serviceName: 'Thuê vợt cầu lông', itemCode: 'EQ-003', condition: 'Damaged', purchaseDate: '2026-04-01T00:00:00Z', purchasePrice: 450000, isAvailable: false },
    { inventoryId: 4, serviceId: 2, serviceName: 'Thuê bóng tennis', itemCode: 'EQ-004', condition: 'Good', purchaseDate: '2026-05-15T00:00:00Z', purchasePrice: 200000, isAvailable: true },
    { inventoryId: 5, serviceId: 2, serviceName: 'Thuê bóng tennis', itemCode: 'EQ-005', condition: 'Retired', purchaseDate: '2025-12-10T00:00:00Z', purchasePrice: 180000, isAvailable: false }
  ];
  localStorage.setItem('mock_equipment_inventory', JSON.stringify(defaults));
  return defaults;
};

const saveMockEquipment = (list: any[]) => {
  localStorage.setItem('mock_equipment_inventory', JSON.stringify(list));
};

// Helper to load/save mock customers in LocalStorage
const getMockCustomers = async (): Promise<any[]> => {
  const local = localStorage.getItem('mock_customers_list');
  if (local) return JSON.parse(local);

  // Fallback to static users mock
  const { default: users } = await import('@/mocks/users.json');
  // Filter for Customers
  const customerList = (users as any[]).filter(u => u.role === 'Customer').map(u => ({
    userId: u.userId,
    fullName: u.fullName,
    email: u.email,
    phone: u.phone,
    avatarUrl: u.avatarUrl,
    loyaltyPoints: u.loyaltyPoints,
    membershipTierId: u.membershipTierId,
    membershipTierName: u.membershipTierName,
    isActive: u.isActive,
    gender: 'Male',
    skillLevel: 'Beginner',
    createdAt: u.createdAt
  }));
  localStorage.setItem('mock_customers_list', JSON.stringify(customerList));
  return customerList;
};

const saveMockCustomers = (list: any[]) => {
  localStorage.setItem('mock_customers_list', JSON.stringify(list));
};

const getMockServices = () => [
  { serviceId: 1, serviceName: 'Thuê vợt cầu lông', category: 'Equipment', price: 30000 },
  { serviceId: 2, serviceName: 'Thuê bóng tennis', category: 'Equipment', price: 15000 },
  { serviceId: 3, serviceName: 'Thuê giày thể thao', category: 'Equipment', price: 20000 }
];

// --- API IMPLEMENTATIONS ---

// 1. Staff stats
export async function getStaffStats(): Promise<{ stats: any; recentBookings: any[] }> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const eqList = getMockEquipment();
    const custList = await getMockCustomers();

    const stats = {
      totalCustomers: custList.length,
      totalEquipments: eqList.length,
      todayBookings: 3,
      availableCourts: 4
    };

    const recentBookings = [
      { bookingId: 101, bookingCode: 'BK-9912', customerName: 'Nguyễn Văn Hùng', courtName: 'Sân Cầu Lông A1', slotName: 'Giờ vàng', totalAmount: 150000, status: 'Confirmed' },
      { bookingId: 102, bookingCode: 'BK-9913', customerName: 'Lê Minh Tuấn', courtName: 'Sân Tennis D1', slotName: 'Buổi chiều', totalAmount: 300000, status: 'Pending' },
      { bookingId: 103, bookingCode: 'BK-9914', customerName: 'Phan Văn Đức', courtName: 'Sân Bóng Đá B1', slotName: 'Cuối tuần tối', totalAmount: 600000, status: 'Confirmed' }
    ];

    return { stats, recentBookings };
  }

  const response = await axiosInstance.get<{ data: any; recentBookings: any[] }>('/staffdashboard/stats');
  return {
    stats: response.data.data,
    recentBookings: (response.data as any).recentBookings || []
  };
}

// 2. Equipment CRUD
export async function getEquipment(): Promise<any[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    return getMockEquipment();
  }
  const response = await axiosInstance.get<{ data: any[] }>('/equipmentinventory');
  return response.data.data;
}

export async function addEquipment(payload: any): Promise<any> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const list = getMockEquipment();
    const services = getMockServices();
    const matchedService = services.find(s => s.serviceId === Number(payload.serviceId));

    const newItem = {
      inventoryId: list.length > 0 ? Math.max(...list.map(x => x.inventoryId)) + 1 : 1,
      serviceId: Number(payload.serviceId),
      serviceName: matchedService?.serviceName || 'Dịch vụ tùy chọn',
      itemCode: payload.itemCode,
      condition: payload.condition,
      purchaseDate: payload.purchaseDate,
      purchasePrice: Number(payload.purchasePrice),
      isAvailable: payload.isAvailable
    };

    if (list.some(x => x.itemCode === payload.itemCode)) {
      throw new Error(`Mã dụng cụ '${payload.itemCode}' đã tồn tại.`);
    }

    list.push(newItem);
    saveMockEquipment(list);
    return newItem;
  }

  const response = await axiosInstance.post<{ data: any }>('/equipmentinventory', payload);
  return response.data.data;
}

export async function updateEquipment(id: number, payload: any): Promise<any> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const list = getMockEquipment();
    const services = getMockServices();
    const matchedService = services.find(s => s.serviceId === Number(payload.serviceId));

    const index = list.findIndex(x => x.inventoryId === id);
    if (index === -1) throw new Error('Không tìm thấy dụng cụ cần cập nhật.');

    if (list.some(x => x.itemCode === payload.itemCode && x.inventoryId !== id)) {
      throw new Error(`Mã dụng cụ '${payload.itemCode}' đã tồn tại.`);
    }

    const updatedItem = {
      ...list[index],
      serviceId: Number(payload.serviceId),
      serviceName: matchedService?.serviceName || 'Dịch vụ tùy chọn',
      itemCode: payload.itemCode,
      condition: payload.condition,
      purchaseDate: payload.purchaseDate,
      purchasePrice: Number(payload.purchasePrice),
      isAvailable: payload.isAvailable
    };

    list[index] = updatedItem;
    saveMockEquipment(list);
    return updatedItem;
  }

  const response = await axiosInstance.put<{ data: any }>(`/equipmentinventory/${id}`, payload);
  return response.data.data;
}

export async function deleteEquipment(id: number): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    const list = getMockEquipment();
    const filtered = list.filter(x => x.inventoryId !== id);
    saveMockEquipment(filtered);
    return;
  }
  await axiosInstance.delete(`/equipmentinventory/${id}`);
}

export async function getServices(): Promise<any[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 200));
    return getMockServices();
  }
  const response = await axiosInstance.get<{ data: any[] }>('/equipmentinventory/services');
  return response.data.data;
}

// 3. Customer CRUD
export async function getCustomers(search?: string): Promise<any[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const list = await getMockCustomers();
    if (!search) return list;
    const term = search.toLowerCase();
    return list.filter(c => c.fullName.toLowerCase().includes(term) || c.email.toLowerCase().includes(term) || c.phone.includes(term));
  }
  const response = await axiosInstance.get<{ data: any[] }>('/customers', { params: { search } });
  return response.data.data;
}

export async function addCustomer(payload: any): Promise<any> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 500));
    const list = await getMockCustomers();

    if (list.some(c => c.email.toLowerCase() === payload.email.toLowerCase())) {
      throw new Error('Email này đã được sử dụng.');
    }

    const tiers = ['Bronze', 'Silver', 'Gold', 'Platinum'];
    const selectedTierId = Number(payload.membershipTierId) || 1;
    const tierName = tiers[selectedTierId - 1] || 'Bronze';

    const newCust = {
      userId: list.length > 0 ? Math.max(...list.map(x => x.userId)) + 1 : 100,
      fullName: payload.fullName,
      email: payload.email,
      phone: payload.phone || '',
      avatarUrl: `https://api.dicebear.com/8.x/avataaars/svg?seed=${payload.fullName}`,
      loyaltyPoints: Number(payload.loyaltyPoints) || 0,
      membershipTierId: selectedTierId,
      membershipTierName: tierName,
      isActive: payload.isActive !== false,
      gender: payload.gender || 'Other',
      skillLevel: payload.skillLevel || 'Beginner',
      createdAt: new Date().toISOString()
    };

    list.push(newCust);
    saveMockCustomers(list);
    return newCust;
  }

  const response = await axiosInstance.post<{ data: any }>('/customers', payload);
  return response.data.data;
}

export async function updateCustomer(id: number, payload: any): Promise<any> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    const list = await getMockCustomers();
    const index = list.findIndex(x => x.userId === id);
    if (index === -1) throw new Error('Không tìm thấy khách hàng.');

    const tiers = ['Bronze', 'Silver', 'Gold', 'Platinum'];
    const selectedTierId = Number(payload.membershipTierId) || 1;
    const tierName = tiers[selectedTierId - 1] || 'Bronze';

    const updated = {
      ...list[index],
      fullName: payload.fullName,
      phone: payload.phone || '',
      loyaltyPoints: Number(payload.loyaltyPoints),
      membershipTierId: selectedTierId,
      membershipTierName: tierName,
      isActive: payload.isActive,
      gender: payload.gender,
      skillLevel: payload.skillLevel
    };

    list[index] = updated;
    saveMockCustomers(list);
    return updated;
  }

  const response = await axiosInstance.put<{ data: any }>(`/customers/${id}`, payload);
  return response.data.data;
}

export async function toggleCustomerStatus(id: number): Promise<boolean> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    const list = await getMockCustomers();
    const index = list.findIndex(x => x.userId === id);
    if (index === -1) throw new Error('Không tìm thấy khách hàng.');
    
    list[index].isActive = !list[index].isActive;
    saveMockCustomers(list);
    return list[index].isActive;
  }
  const response = await axiosInstance.put<{ status: boolean }>(`/customers/${id}/status`);
  return response.data.status;
}
