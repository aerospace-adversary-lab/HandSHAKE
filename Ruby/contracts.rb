require 'eth'
require 'json'
require 'time'

# Connect to the Ethereum client
client = Eth::Client.create('http://127.0.0.1:7545')
puts "Connected to Ethereum client"
puts "Gas price: #{client.eth_gas_price['result'].to_i(16)}"

# Load the contract ABI and bytecode
puts "Loading contract ABI and bytecode"
abi = File.read('../contracts/build/Tester.abi')
bytecode = File.read('../contracts/build/Tester.bin')

if abi.nil? || bytecode.nil?
  puts "Error: ABI or bytecode file is missing."
  exit
end



contract = Eth::Contract.from_file(file: '../contracts/Tester.sol')

puts "Contract ABI loaded: #{abi}"
puts "Contract bytecode loaded: #{bytecode}"

# Deploy the contract
def deploy_contract(client, bytecode, proposer_account, proposer_key, constructor_params)
  puts "Encoding constructor parameters"
  # Encode constructor parameters
  encoded_params = Eth::Abi.encode(constructor_params[:types], constructor_params[:values])
  deploy_data = bytecode + encoded_params
  puts "bytesize #{deploy_data.bytesize}"
  puts "Constructor parameters encoded: "

  balance = client.eth_get_balance(proposer_account)
  puts "Deployer account balance: #{balance['result'].to_i(16) / 1e18} ETH"
  # Retrieve gas price
  gas_price = client.max_fee_per_gas + client.max_priority_fee_per_gas
  puts "Gas price cretrieved: #{gas_price}"

  # Create the deployment transaction
  tx_params = {
    from: proposer_account,
    data: deploy_data,
    gas_limit: 79_999_000_000,
    gas_price: gas_price,
    nonce: client.get_nonce(proposer_account)
  }
  puts "Deployment transaction params: "

  # Use Eth::Tx to create a new transaction
  tx = Eth::Tx.new(tx_params)
  tx.sign(proposer_key)
  puts "Signed deployment transaction"

  begin
    puts "hello"
    tx_hash = clclient.transact_and_wait(simplestorage_contract, "set", 1234, sender_key: deployer_account)
    puts "hi"
    puts "Transaction hash: #{tx_hash['result']}"
  rescue => e
    puts "Error during deployment: #{e.message}"
    exit
  end
  puts "Deployment transaction hash: #{tx_hash}"

  # Wait for the transaction to be mined
  loop do
    receipt_payload = {
      jsonrpc: "2.0",
      method: "eth_getTransactionReceipt",
      params: [tx_hash],
      id: 1
    }
    receipt_response = client.send_request(receipt_payload.to_json)
    receipt = JSON.parse(receipt_response)
    if receipt['result']
      puts "Gas used: #{receipt['result']['gasUsed']}"
    else
      puts "Transaction receipt not yet available."
    end

    break receipt unless receipt['result'].nil?

    sleep 1
  end

  receipt['result']['contractAddress']
end

# Get gas price
def get_gas_price(client)
  client.eth_gas_price['result'].to_i(16)
end

# Update data in the contract
def update_data(client, contract, account, key, new_data)
  puts "Updating data..."
  data = contract.functions['updateData'].encode_input(new_data)
  puts "Data to update: #{data}"

  gas_price = get_gas_price(client)
  intrinsic_gas = Eth::Tx.estimate_intrinsic_gas(data)

  tx = Eth::Tx.new({
    from: account,
    to: contract.address,
    data: data,
    gas_limit: intrinsic_gas,
    gas_price: gas_price,
    nonce: client.get_nonce(account),
    chain_id: 1337 # Chain ID for Ganache
  })

  tx.sign(key)
  puts "Data update transaction signed."

  # Send the signed transaction using send_request
  payload = {
    jsonrpc: "2.0",
    method: "eth_sendRawTransaction",
    params: ["0x" + tx.hex],
    id: 1
  }
  response = client.send_request(payload.to_json)
  output = JSON.parse(response)
  raise IOError, output["error"]["message"] unless output["error"].nil?
  tx_hash = output['result']
  puts "Data update transaction sent. Transaction hash: #{tx_hash}"
end

# Update requester validation in the contract
def update_requester_validation(client, contract, account, key, validation)
  puts "Updating requester validation..."
  data = contract.functions['updateRequesterValidation'].encode_input(validation)
  puts "Validation data: #{data}"

  gas_price = get_gas_price(client)
  intrinsic_gas = Eth::Tx.estimate_intrinsic_gas(data)

  tx = Eth::Tx.new({
    from: account,
    to: contract.address,
    data: data,
    gas_limit: intrinsic_gas,
    gas_price: gas_price,
    nonce: client.get_nonce(account),
    chain_id: 1337 # Chain ID for Ganache
  })

  tx.sign(key)
  puts "Requester validation transaction signed."

  # Send the signed transaction using send_request
  payload = {
    jsonrpc: "2.0",
    method: "eth_sendRawTransaction",
    params: ["0x" + tx.hex],
    id: 1
  }
  response = client.send_request(payload.to_json)
  output = JSON.parse(response)
  raise IOError, output["error"]["message"] unless output["error"].nil?
  tx_hash = output['result']
  puts "Requester validation transaction sent. Transaction hash: #{tx_hash}"
end

# Retrieve message from the contract
def get_message(contract)
  puts "Retrieving message..."
  message = contract.functions['getMessage'].call
  puts "Message retrieved: #{message}"
  message
end

# Define the proposer and requester accounts and keys
proposer_account = '0x47467E20117585087fFd05f4116DaDE07f6C7129'
requester_account = '0x0665b7f874cb05B4CFb2C947D6679E7b2BE8051a'
proposer_private_key = '0xae3beae975e0d4d9c55c24f967c0ed16731c08f2f425f1b6c1574d5a13081547'
requester_private_key = '0x8d75fef38f85bbdd36f537a1fab57b41d2521bf53da5af67a71e93dcf3f2b675'

proposer_key = Eth::Key.new priv: proposer_private_key
requester_key = Eth::Key.new priv: requester_private_key

# Define the constructor parameters
constructor_params = {
  types: ['address', 'address', 'string', 'int256[]', 'string', 'string', 'uint256[2]'],
  values: [proposer_account, requester_account, 'ground', [1, 2], '', '', [1622476800, 1622563200]]
}

puts "Deploying the contract..."
# Deploy the contract
contract_address = deploy_contract(client, bytecode, proposer_account, proposer_key, constructor_params)
puts "Contract deployed at: #{contract_address}"

# Initialize the contract object with the deployed address
contract = Eth::Contract.from_abi(name: 'TargetContract', address: contract_address, abi: abi)

puts "Retrieving message from the contract..."
# Retrieve the message to verify it was added during deployment
message = get_message(contract)
puts "Message: #{message}"

puts "Proposer updating the data..."
# Proposer updates the data
update_data(client, contract, proposer_account, proposer_key, 'new_data_here')

puts "Requester updating the requester validation..."
# Requester updates the requester validation
update_requester_validation(client, contract, requester_account, requester_key, true)

puts "Script completed successfully."
