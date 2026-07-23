export interface UserManagementResponse {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export interface UpdateUserRoleRequest {
  roleName: string;
}
