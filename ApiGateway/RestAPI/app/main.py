from functools import wraps
from flask import Flask, request, make_response, abort, jsonify, Blueprint
from flask_restplus import Resource, Api, fields, Namespace
import psycopg2
import uuid
import requests
import json

app = Flask(__name__)
blueprint = Blueprint('api', __name__, url_prefix='/api')

users = Namespace(name="users", description="API for creating users")
stocks = Namespace(name="stocks", description="API for managing stocks")
logs = Namespace(name="logs", description="Administrator API for fetching logs")

ownercontrol_url = 'http://10.0.0.110:5000/api'
provider_url = 'http://10.0.0.111:5000/api'
requester_url = 'http://10.0.0.112:5000/api'

api = Api(blueprint, version='1.0', title='TSEIS API', doc='/swagger/', default="TSEIS", default_label="The RestAPI for ITONK-TSEIS")
app.register_blueprint(blueprint)
api.add_namespace(users)
api.add_namespace(stocks)
api.add_namespace(logs)

users_database_connectionstring = "user='postgres' password='password' host='10.0.0.93' dbname='tseis'"
log_database_connectionstring = "user='postgres' password='password' host='10.0.0.50' dbname='tseis'"

def requires_auth(f):
	@wraps(f)
	def decorated(*args, **kwargs):
		print("authenticating request")
		username = request.headers.get('tseis-username')
		password = request.headers.get('tseis-password')
		if not username or not password:
			abort(401, "No credentials provided")
		userid = get_userid(username, password)
		if not userid:
			abort(401, "Invalid username or password")

		request.authenticated_user = userid
		return f(*args, **kwargs)
	return decorated

def username_taken(username):
	conn = psycopg2.connect(users_database_connectionstring)
	cur = conn.cursor()

	cur.execute('select id from users where username=%s', (username,))
	userid = cur.fetchone()

	cur.close()
	conn.close()

	return userid is not None

def create_user(userid, username, password):
	conn = psycopg2.connect(users_database_connectionstring)
	cur = conn.cursor()

	cur.execute('insert into users (id, username, password) values (%s, %s, %s);', (userid, username, password))

	conn.commit()
	cur.close()
	conn.close()

def get_userid(username, password):
	conn = psycopg2.connect(users_database_connectionstring)
	cur = conn.cursor()

	cur.execute('select id from users where username=%s and password=%s', (username, password))
	userid = cur.fetchone()

	cur.close()
	conn.close()
	return None if userid is None else userid[0]

def get_logs():
	conn = psycopg2.connect(log_database_connectionstring)
	cur = conn.cursor()

	cur.execute('select * from logs')
	logs = cur.fetchall()

	cur.close()
	conn.close()
	return logs

@requires_auth
def api_get(url):
	headers = {"AuthenticatedUser" : request.authenticated_user}
	print('GET ' + url + ' with headers: ' + str(headers))
	r = requests.get(url, headers=headers)
	print('Status code: ' + str(r.status_code) + ' and body: ' + r.text)
	return r.text, r.status_code

@requires_auth
def api_post(url):
	headers = {"AuthenticatedUser" : request.authenticated_user}
	json_body = request.get_json(force=True)
	print('POST ' + url + ' with headers: ' + str(headers) + ' and body: ' + str(json_body))
	r = requests.post(url, headers=headers, data=json_body)
	print('Status code: ' + str(r.status_code) + ' and body: ' + r.text)
	return r.text, r.status_code

@requires_auth
def api_put(url):
	headers = {"AuthenticatedUser" : request.authenticated_user}
	json_body = request.get_json(force=True)
	print('PUT ' + url + ' with headers: ' + str(headers) + ' and body: ' + str(json_body))
	r = requests.put(url, headers=headers, data=json_body)
	print('Status code: ' + str(r.status_code) + ' and body: ' + r.text)
	return r.text, r.status_code

# --- APIS ---

@users.route('/', methods = ['POST'])
class UsersApi(Resource):
	user_input = api.model('User information', {
	    'username': fields.String,
	    'password': fields.String
	})

	@api.response(201, "User was created")
	@api.response(400, "Username already taken")
	@api.doc(body=user_input)
	def post(self):
		body = request.get_json(force=True)
		if not body:
			abort(400, "No body provided")
		username = body['username']
		if username_taken(username):
			abort(400, "Username already taken")

		password = body['password']
		userid = uuid.uuid4()
		create_user(str(userid), username, password)
		return None, 201

@api.response(401, "Invalid credentials")
@api.header('tseis-username', 'Your username', required=True)
@api.header('tseis-password', 'Your password', required=True)
@stocks.route('/mine', methods=['GET'])
class OwnStocks(Resource):
	@api.response(200, "Successfully retrieved your stocks")
	def get(self):
		return api_get(ownercontrol_url + 'users/me/stocks')		

@api.response(401, "Invalid credentials")
@api.header('tseis-username', 'Your username', required=True)
@api.header('tseis-password', 'Your password', required=True)
@stocks.route('/mine/sell', methods=['PUT'])
class SellStocks(Resource):
	sell_input = api.model('Stocks to sell', {
	    'name': fields.String,
	    'price': fields.Float,
	    'amount': fields.Integer
	})

	@api.response(202, "Stock successfully set for sale")
	@api.doc(body=sell_input)
	def put(self):
		return api_put(provider_url + '/stocks/sell')

@api.response(401, "Invalid credentials")
@api.header('tseis-username', 'Your username', required=True)
@api.header('tseis-password', 'Your password', required=True)
@stocks.route('/forsale', methods=['GET'])
class StocksForSale(Resource):
	@api.response(200, "Successfully retrieved stocks for sale")
	def get(self):
		return api_get(requester_url + '/stocks/forsale')

@api.response(401, "Invalid credentials")
@api.header('tseis-username', 'Your username', required=True)
@api.header('tseis-password', 'Your password', required=True)
@stocks.route('/forsale/buy', methods=['PUT'])
class BuyStocks(Resource):
	buy_input = api.model('Stocks to sell', {
	    'name': fields.String,
	    'price': fields.Float,
	    'amount': fields.Integer
	})
	
	@api.response(202, "Stock offer successfully placed")
	@api.doc(body=buy_input)
	def put(self):
		return api_put(requester_url + '/stocks/forsale/buy')

@api.response(401, "Invalid credentials")
@api.header('tseis-username', 'Your username (requires administrator rights)', required=True)
@api.header('tseis-password', 'Your password', required=True)
@logs.route('/', methods=['GET'])
class GetLogs(Resource):
	@api.response(200, "Logs successfully fetched")
	def get(self):
		username = request.headers.get('tseis-username')
		password = request.headers.get('tseis-password')
		if username != "admin" or password != "admin":
			abort(401, "Invalid or no credentials")

		logs = get_logs()
		logs = [{'time': str(x[4]), 'service': x[1], 'message':x[2]} for x in logs]
		return logs, 200

if __name__ == '__main__':
	app.run(host='0.0.0.0', port=80, debug=True)