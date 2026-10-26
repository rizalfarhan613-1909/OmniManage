/** Mirrors `BusinessPartnerDto` (MasterData module). */
export interface BusinessPartner {
  id: string;
  code: string;
  name: string;
  partnerType: 'Customer' | 'Vendor' | 'Both';
  taxIdMasked: string | null;
  creditLimit: string;
  isActive: boolean;
  createdAt: string;
}

/** Mirrors `ItemDto` (MasterData module). */
export interface Item {
  id: string;
  sku: string;
  name: string;
  uomCode: string;
  category: string | null;
  basePrice: string;
  costPrice: string;
  isStockTracked: boolean;
  isActive: boolean;
  createdAt: string;
}
