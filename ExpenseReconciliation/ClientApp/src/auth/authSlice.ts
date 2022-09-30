import {createAsyncThunk, createSlice} from '@reduxjs/toolkit';
import {RootState} from '../store';
import {AuthStatus} from './authStatus';
import {getCurrentUser} from '../api/userClient';
import Session from '../model/session';

const initialState: Session = {
  status: AuthStatus.Unknown,
  user: null,
  token: null,
}

const authSlice = createSlice({
  name: 'auth',
  initialState: initialState,
  reducers: {
    logout: (state) => {
      console.log('[Auth] Log out');
      localStorage.removeItem('token');
      return initialState;
    },
    unauthenticated: (state) => {
      console.log('[Auth] Attempted to access page without authentication');
    }
  },
  extraReducers(builder) {
    builder
      .addCase(getUser.pending, (state, action) => {
        state.status = AuthStatus.Pending;
      })
      .addCase(getUser.fulfilled, (state, action) => {
        state.status = AuthStatus.LoggedIn;
        state.user = action.payload;
      })
      .addCase(getUser.rejected, (state, action) => {
        state.status = AuthStatus.Unauthenticated;
      })
  }
});

export const selectAuthState = (state: RootState) => state.auth.status;
export const selectLoggedIn = (state: RootState) => state.auth.status === AuthStatus.LoggedIn;
export const selectUser = (state: RootState) => state.auth.user;
export const selectToken = (state: RootState) => state.auth.token;

export const { logout, unauthenticated } = authSlice.actions;

export const login = createAsyncThunk('auth/login', async (token: any, thunkAPI) => {
  console.log(`[Auth] New login with token ${token}`);

  localStorage.setItem('token', token);
  
  thunkAPI.dispatch(getUser());
});

export const getUser = createAsyncThunk('auth/getUser', async () => {
  return await getCurrentUser();
});

export default authSlice.reducer;