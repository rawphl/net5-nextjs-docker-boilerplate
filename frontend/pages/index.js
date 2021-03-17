export default ({ data }) => (
	<main>
		<h1>Welcome to net5, next.js and docker!</h1>
		<pre>
			{JSON.stringify(data, null, 2)}
		</pre>
	</main>
)

export async function getServerSideProps(context) {
	console.log("LOADING FROM API SAADSd")
	const response = await fetch(process.env.API_BASE_URL + "/api/comments", { mode: "cors", credentials: "include"})
	const data = await response.json()
	return {
	  props: { data }, 
	}
  }