require 'eth'
require 'forwardable'

client = Eth::Client.create('http://127.0.0.1:8545')
contract = Eth::Contract.from_file(file: '../contracts/TargetContract.sol')

tester_address = "0xd7ff5F6Bc02Bb47639B26344Dfa37213B1b2964A" # Deployed contract address
tester_contract = Eth::Contract.from_abi(name: "TargetContract", address: tester_address, abi: contract.abi)

proposer_account = Eth::Key.new(priv: '0x4155bfcffbe7900c3ee6f997fb568104eb55b1fcef1bf4d51d58765b76892f77')
balance = client.eth_get_balance(proposer_account.address)
puts "proposer account balance: #{balance['result'].to_i(16) / 1e18} ETH"
client.max_fee_per_gas = 41_000_000_000
client.max_priority_fee_per_gas = 4_000_000_000

# Helper function to call contract methods and handle errors
def call_contract_method(client, contract, method, *args)
  begin
    response = client.call(contract, method, *args)
    puts "#{method}: #{response}"
  rescue StandardError => e
    puts "Error while calling #{method}: #{e.message}"
    puts e.backtrace
  end
end

# Call the addMessage function
targetType = 1
groundCoordinates = [0, 0]
tleLine1 = 1234567890
tleLine2 = 987654321
timeWindow = [1657812000, 1657898400] # Replace with appropriate Unix timestamps
requesterId = proposer_account.address

begin
  tx_hash = client.transact_and_wait(tester_contract, "addMessage", targetType, groundCoordinates, tleLine1, tleLine2, timeWindow, requesterId, sender_key: proposer_account, gas_limit: 10_000_000)
  puts "Transaction hash for addMessage: #{tx_hash}"
rescue StandardError => e
  puts "Error while calling addMessage: #{e.message}"
  puts e.backtrace
end

# Call the updateData function
data_hex = 'Finally done'

begin
  tx_hash = client.transact_and_wait(tester_contract, "updateData", data_hex, sender_key: proposer_account, gas_limit: 10_000_000)
  puts "Transaction hash for updateData: #{tx_hash}"
rescue StandardError => e
  puts "Error while calling updateData: #{e.message}"
  puts e.backtrace
end

begin
  tx_hash = client.transact_and_wait(tester_contract, "updateRequesterValidation", true, sender_key: proposer_account, gas_limit: 10_000_000)
  puts "Transaction hash for updateRequesterValidation: #{tx_hash}"
rescue StandardError => e
  puts "Error while calling updateRequesterValidation: #{e.message}"
  puts e.backtrace
end



# Call the updateRequesterValidation function
begin
  response = client.call(tester_contract, "getMessage")
  puts "Current message: #{response}"
rescue StandardError => e
  puts "Error while getting number: #{e.message}"
  puts e.backtrace
end

