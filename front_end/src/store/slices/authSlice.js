import { createSlice } from "@reduxjs/toolkit";
const slice = createSlice({
  name: "auth",
  initialState: { user: null, token: null },
  reducers: {
    setAuth: (s, a) => ({ ...s, ...a.payload }),
    logout: (s) => ({ user: null, token: null }),
  },
});
export const { setAuth, logout } = slice.actions;
export default slice.reducer;
