const form = document.getElementById('loginForm');
const msg = document.getElementById('msg');
const profileCard = document.getElementById('profileCard');
const profilePre = document.getElementById('profile');
const logoutBtn = document.getElementById('logout');

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  msg.textContent = 'Signing in…';
  const email = document.getElementById('email').value.trim();
  const password = document.getElementById('password').value;
  try {
    const res = await fetch('/api/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ email, password })
    });
    const data = await res.json();
    if (!res.ok) throw new Error(data?.error || 'Login failed');
    msg.textContent = 'Logged in.';
    await loadProfile();
  } catch (err) {
    msg.textContent = err.message;
  }
});

async function loadProfile(){
  try{
    const res = await fetch('/api/me', { credentials: 'include' });
    if(!res.ok) throw new Error('Not authenticated');
    const data = await res.json();
    profilePre.textContent = JSON.stringify(data, null, 2);
    profileCard.hidden = false;
  }catch(e){
    profileCard.hidden = true;
  }
}

logoutBtn?.addEventListener('click', async ()=>{
  await fetch('/api/logout', { method:'POST', credentials:'include' });
  profileCard.hidden = true;
  msg.textContent = 'Logged out.';
});
